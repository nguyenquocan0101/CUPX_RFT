using Services.Interfaces;
using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using AutomaticBrewingCoffee.Services.Utils;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Newtonsoft.Json;
using Services.Base;
using Services.CapRabbitMQ.Messages.Notification;
using Services.CapRabbitMQ.Messages.Order;
using Services.CapRabbitMQ.Messages.Payment;
using Services.CapRabbitMQ.Topics;
using Services.Dtos.Order;
using Services.Dtos.OrderDetail;
using Services.Dtos.Payment;
using Services.VNPay;
using Services.MPOS;
using Services.MPOS.Base;
using Services.MPOS.Data;
using Services.Redis;
using Services.Utils;
using Services.VNPay.Base;

namespace Services.Implements
{
    public class OrderService : BaseService<OrderService>, IOrderService
    {
        private readonly VNPayClient? _vnPayClient;
        private readonly MPOSClient? _mPosClient;
        private readonly ICapPublisher _capPublisher;
        private readonly IRedisService _redisService;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor,
            ICapPublisher capPublisher,
            IRedisService redisService,
            RecyclableMemoryStreamManager memoryStreamManager,
            VNPayClient? vnPayClient = null,
            MPOSClient? mPosClient = null) : base(
            unitOfWork,
            mapper,
            loggerFactory,
            httpContextAccessor
        )
        {
            _vnPayClient = vnPayClient;
            _mPosClient = mPosClient;
            _capPublisher = capPublisher;
            _redisService = redisService;
            _memoryStreamManager = memoryStreamManager;
        }

        public async Task<BaseResult<CreateOrderDto, OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            var kioskLock = Locker.Acquire(createOrderDto.KioskId);

            Console.WriteLine($"[Start] {createOrderDto.KioskId} - {createOrderDto.ClientId} - {DateTime.Now}");

            await kioskLock.WaitAsync();

            Console.WriteLine($"[Lock Acquired] {createOrderDto.KioskId} - {createOrderDto.ClientId} - {DateTime.Now}");

            try
            {
                IsValidDiscountCode(createOrderDto.DiscountCode, out var discountPercent);

                var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                    predicate: x => x.KioskId == createOrderDto.KioskId,
                    include: x => x.Include(x => x.KioskDevices)
                        .ThenInclude(x => x.Device)
                        .ThenInclude(x => x.DeviceIngredientStates)
                        .Include(x => x.Store)
                        .ThenInclude(x => x.Organization)
                );


                if (kiosk is null)
                {
                    return new BaseResult<CreateOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.NotFound<Kiosk>(),
                        StatusCode = StatusCodes.Status404NotFound,
                        Request = createOrderDto,
                        Response = null
                    };
                }

                if (kiosk.Status == nameof(EBaseStatus.Inactive))
                {
                    return new BaseResult<CreateOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.IsPause<Kiosk>(),
                        Request = createOrderDto,
                        Response = null,
                        StatusCode = StatusCodes.Status200OK
                    };
                }

                var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
                    predicate: x =>
                        x.KioskId == kiosk.KioskId && x.WebhookType == nameof(EWebhookType.HealthCheck)
                );

                if (webhook is null)
                {
                    return new BaseResult<CreateOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.NotFound<Webhook>(),
                        StatusCode = StatusCodes.Status404NotFound,
                        Request = createOrderDto,
                        Response = null
                    };
                }

                var resultHealthCheck = await ApiUtil.GetAsync(
                    webhook.WebhookUrl,
                    headers: new Dictionary<string, string>()
                    {
                        { "X-API-KEY", ApiKeyUtil.Decrypt(kiosk.ApiKey!) }
                    }
                );

                if (!resultHealthCheck.IsSuccessStatusCode)
                {
                    var response = new BaseResult<CreateOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Request = createOrderDto,
                        Response = null,
                        Message = MessageUtil.NoResponse<Kiosk>()
                    };

                    var notificationKioskCapMessage = new NotificationKioskCapMessage()
                    {
                        KioskId = kiosk.KioskId,
                        CreatedBy = "System",
                        NotificationType = ENotificationType.KioskNotWorking
                    };

                    switch (resultHealthCheck.StatusCode)
                    {
                        case HttpStatusCode.ServiceUnavailable:
                        {
                            response.Message = MessageUtil.Busy<Kiosk>();
                            notificationKioskCapMessage.NotificationType = ENotificationType.KioskBusy;
                            break;
                        }
                    }

                    await _capPublisher.PublishAsync(NotificationCapTopic.NotificationKiosk,
                        notificationKioskCapMessage);

                    return response;
                }

                // Xử lí các attribute và option trùng lập
                createOrderDto.OrderDetails.ForEach(od => od.Normalize());

                var kioskId = kiosk.KioskId;
                var storeId = kiosk.StoreId;
                var orgCode = kiosk.Store!.Organization!.OrganizationCode;
                var organizationId = kiosk.Store.OrganizationId;

                var orderCode = await OrderCodeHelper.GenerateOrderCodeAsync(
                    _redisService,
                    orgCode,
                    storeId,
                    kioskId
                );

                var newOrder = _mapper.Map<Order>(createOrderDto, opts =>
                {
                    opts.Items["StoreId"] = storeId;
                    opts.Items["KioskId"] = kioskId;
                    opts.Items["OrganizationId"] = organizationId;
                    opts.Items["OrderType"] = nameof(EOrderType.Immediate);
                    opts.Items["OrderCode"] = orderCode;
                });

                var ingredientsAvailableResult = await IngredientHelper.CheckIngredientsAvailableAsync(
                    _unitOfWork,
                    newOrder.OrderId,
                    createOrderDto.OrderDetails,
                    kiosk.KioskDevices.ToList()
                );

                _unitOfWork.GetRepository<DeviceIngredientState>()
                    .UpdateRange(ingredientsAvailableResult.UpdatedStates);

                await _unitOfWork.GetRepository<DeviceIngredientHistory>()
                    .InsertRangeAsync(ingredientsAvailableResult.Histories);

                if (!ingredientsAvailableResult.IsSuccess)
                {
                    var notificationKioskCapMessage = new NotificationKioskCapMessage()
                    {
                        KioskId = kiosk.KioskId,
                        CreatedBy = "System",
                        NotificationType = ENotificationType.KioskNotEnoughIngredient,
                        Delivery = JsonConvert.SerializeObject(ingredientsAvailableResult.MissingIngredients),
                    };

                    await _capPublisher.PublishAsync(NotificationCapTopic.NotificationKiosk,
                        notificationKioskCapMessage);

                    return new BaseResult<CreateOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.NotEnough<DeviceIngredientState>(),
                        StatusCode = StatusCodes.Status406NotAcceptable,
                        Response = null,
                        Request = createOrderDto
                    };
                }

                newOrder.Calculate(discountPercent);
                newOrder.Pending();

                var payment = new Payment()
                {
                    PaymentId = Guid.NewGuid().ToString(),
                    OrderId = newOrder.OrderId,
                    PaymentContent = null,
                    ReferenceId = null,
                    RequiredAmount = newOrder.FinalAmount,
                };


                var orderDto = _mapper.Map<OrderDto>(newOrder, opts =>
                {
                    opts.Items["PaymentQr"] = string.Empty;
                    opts.Items["PaymentId"] = payment.PaymentId;
                    opts.Items["PaymentUrl"] = string.Empty;
                });

                switch (createOrderDto.PaymentGateway)
                {
                    case nameof(EPaymentGateway.VNPay):
                    {
                        if (newOrder.FinalAmount <= 0)
                        {
                            payment.Pending(DateTime.UtcNow.AddMinutes(15));
                            orderDto.ExpiredDate = payment.ExpiredDate;
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
                            await _unitOfWork.CommitAsync();

                            await _capPublisher.PublishDelayAsync(
                                TimeSpan.FromSeconds(3),
                                PaymentCapTopic.PaymentVNPAYCallback,
                                new PaymentVNPAYCallbackMessage()
                                {
                                    TmnCode = "",
                                    Amount = (long)newOrder.FinalAmount,
                                    BankCode = "",
                                    BankTranNo = "",
                                    CardType = "",
                                    PayDate = DateTime.UtcNow.ToString(),
                                    PayDateParsed = DateTime.UtcNow,
                                    OrderInfo = newOrder.OrderId,
                                    TransactionNo = "",
                                    ResponseCode = "",
                                    TransactionStatus = "00",
                                    TxnRef = "",
                                    SecureHash = "",
                                    TransactionStatusEnum = nameof(VNPayTransStatus.Success)
                                });
                        }
                        else
                        {
                            var result = _vnPayClient.CreatePaymentUrl(payment);
                            orderDto.PaymentUrl = result;
                            payment.Pending(DateTime.UtcNow.AddMinutes(15));
                            orderDto.ExpiredDate = payment.ExpiredDate;
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
                            await _unitOfWork.CommitAsync();
                        }

                        break;
                    }
                    case nameof(EPaymentGateway.RESO):
                    {
                        // Reso payment here
                        payment.Pending(DateTime.UtcNow.AddMinutes(10));
                        orderDto.ExpiredDate = payment.ExpiredDate;
                        await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                        await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
                        await _unitOfWork.CommitAsync();
                        break;
                    }
                    case nameof(EPaymentGateway.MPOS):
                    {
                        if (newOrder.FinalAmount <= 0)
                        {
                            payment.Pending(DateTime.UtcNow.AddMinutes(15));
                            orderDto.ExpiredDate = payment.ExpiredDate;
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
                            await _unitOfWork.CommitAsync();

                            await _capPublisher.PublishDelayAsync(
                                TimeSpan.FromSeconds(3),
                                PaymentCapTopic.PaymentMPOSCallback,
                                new PaymentMPOSCallbackMessage()
                                {
                                    TranStatusEnum = nameof(MPOSTransStatus.Approved),
                                    TransStatus = (long)MPOSTransStatus.Approved,
                                    OrderId = newOrder.OrderId,
                                    PosId = "",
                                    Muid = "",
                                    IssuerCode = "",
                                    ServiceName = "",
                                    TransAmount = 0,
                                    TransDate = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                                    TransCode = "",
                                });
                        }
                        else
                        {
                            var mPosPaymentInfo =
                                await _mPosClient.CreateQRPayment(newOrder.OrderId,
                                    newOrder.FinalAmount?.ToString() ?? "0",
                                    createOrderDto.Content);
                            orderDto.PaymentQr = mPosPaymentInfo.QrCode;
                            payment.Pending(DateTime.UtcNow.AddMinutes(2));
                            orderDto.ExpiredDate = payment.ExpiredDate;
                            await _unitOfWork.GetRepository<Order>().InsertAsync(newOrder);
                            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
                            await _unitOfWork.CommitAsync();
                        }


                        break;
                    }
                }

                return new BaseResult<CreateOrderDto, OrderDto>()
                {
                    Message = MessageUtil.CreateSuccess<Order>(),
                    Request = createOrderDto,
                    Response = orderDto,
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Console.WriteLine($"[Done] {createOrderDto.KioskId} - {createOrderDto.ClientId} - {DateTime.Now}");
                kioskLock.Release();
            }
        }

        public async Task<BaseResult<CancelOrderDto, OrderDto>> CancelOrder(CancelOrderDto cancelOrderDto)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == cancelOrderDto.OrderId,
                include: x => x.Include(x => x.OrderDetails)
            );

            // Order not found
            if (order is null)
            {
                return new BaseResult<CancelOrderDto, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Order>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = cancelOrderDto,
                    Response = null
                };
            }

            // Check the order status first
            switch (order.Status)
            {
                case nameof(EOrderStatus.Pending):
                {
                    // Valid to cancel
                    break;
                }
                case nameof(EOrderStatus.Preparing):
                {
                    // Order is making by kiosk can not cancel
                    return new BaseResult<CancelOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Preparing),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = cancelOrderDto
                    };
                }
                case nameof(EOrderStatus.Cancelled):
                {
                    // Order is already cancel
                    return new BaseResult<CancelOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Cancelled),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = cancelOrderDto
                    };
                }
                case nameof(EOrderStatus.Failed):
                {
                    // Order is already failed
                    return new BaseResult<CancelOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Failed),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = cancelOrderDto
                    };
                }
                case nameof(EOrderStatus.Completed):
                {
                    // Order is already complete
                    return new BaseResult<CancelOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Completed),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = cancelOrderDto
                    };
                }
            }

            // KioskId not match
            if (!order.KioskId.Equals(cancelOrderDto.KioskId))
            {
                return new BaseResult<CancelOrderDto, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<Order>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = cancelOrderDto,
                    Response = null
                };
            }

            // ClientId not match
            if (!order.ClientId.Equals(cancelOrderDto.ClientId))
            {
                return new BaseResult<CancelOrderDto, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<Order>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = cancelOrderDto,
                    Response = null
                };
            }

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == cancelOrderDto.KioskId,
                include: x => x.Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
            );

            if (kiosk is null)
            {
                return new BaseResult<CancelOrderDto, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotEnough<Kiosk>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = cancelOrderDto,
                    Response = null
                };
            }

            switch (order.PaymentGateway)
            {
                case nameof(EPaymentGateway.VNPay):
                {
                    break;
                }
                case nameof(EPaymentGateway.MPOS):
                {
                    await _mPosClient.CancelQRPayment(orderId: order.OrderId, order.FinalAmount.ToString() ?? "0");
                    break;
                }
            }

            order.Cancelled("Customer");

            var payment = new Payment()
            {
                PaymentId = Guid.NewGuid().ToString(),
                OrderId = order.OrderId,
                PaymentContent = "Cancel by customer",
                ReferenceId = null,
                CreateBy = $"Customer from {GetKioskIdFromJwt()}",
                RequiredAmount = order.FinalAmount,
            };
            payment.Cancelled();

            var ingredientsToRestore = await IngredientHelper.RestoreIngredientsFromOrderAsync(
                _unitOfWork,
                order.OrderId,
                order.OrderDetails.ToList(),
                kiosk.KioskDevices.ToList()
            );

            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            _unitOfWork.GetRepository<Order>().Update(order);
            _unitOfWork.GetRepository<DeviceIngredientState>().UpdateRange(ingredientsToRestore.UpdatedStates);
            await _unitOfWork.GetRepository<DeviceIngredientHistory>().InsertRangeAsync(ingredientsToRestore.Histories);

            await _unitOfWork.CommitAsync();

            return new BaseResult<CancelOrderDto, OrderDto>();
        }

        public async Task<BaseResult<RefundOrderDto, OrderDto>> RefundOrder(string orderId,
            RefundOrderDto refundOrderDto)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == orderId
            );

            // Order not found
            if (order is null)
            {
                return new BaseResult<RefundOrderDto, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Order>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = refundOrderDto,
                    Response = null
                };
            }

            // Check the order status first
            switch (order.Status)
            {
                case nameof(EOrderStatus.Pending):
                {
                    // Order is pending by kiosk can not cancel
                    return new BaseResult<RefundOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Pending),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = refundOrderDto
                    };
                }
                case nameof(EOrderStatus.Preparing):
                {
                    // Order is making by kiosk can not cancel
                    return new BaseResult<RefundOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Preparing),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = refundOrderDto
                    };
                }
                case nameof(EOrderStatus.Cancelled):
                {
                    // Order is already cancel
                    return new BaseResult<RefundOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Cancelled),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = refundOrderDto
                    };
                }
                case nameof(EOrderStatus.Failed):
                {
                    // valid to refund
                    break;
                }
                case nameof(EOrderStatus.Completed):
                {
                    return new BaseResult<RefundOrderDto, OrderDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.OrderStatusError(EOrderStatus.Completed),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Response = null,
                        Request = refundOrderDto
                    };
                }
            }

            if (refundOrderDto.RefundAmount > order.TotalAmount)
            {
                return new BaseResult<RefundOrderDto, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<RefundOrderDto>(),
                    StatusCode = StatusCodes.Status400BadRequest,
                    Response = null,
                    Request = refundOrderDto
                };
            }

            if (order.FinalAmount <= 0)
            {
                return new BaseResult<RefundOrderDto, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<RefundOrderDto>(),
                    StatusCode = StatusCodes.Status400BadRequest,
                    Response = null,
                    Request = refundOrderDto
                };
            }

            var payment = new Payment()
            {
                PaymentId = Guid.NewGuid().ToString(),
                OrderId = order.OrderId,
                PaymentContent = refundOrderDto.Content,
                ReferenceId = null,
                RequiredAmount = order.FinalAmount,
            };

            switch (order.PaymentGateway)
            {
                case nameof(EPaymentGateway.VNPay):
                {
                    payment.Refunded(refundOrderDto.RefundAmount ?? order.TotalAmount);
                    break;
                }
                case nameof(EPaymentGateway.MPOS):
                {
                    var refundResponse = await _mPosClient.RefundPaidPayment(orderId,
                        (long)(refundOrderDto.RefundAmount ?? order.TotalAmount)!);
                    payment.Refunded(refundOrderDto.RefundAmount ?? order.TotalAmount);
                    break;
                }
            }

            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            await _unitOfWork.CommitAsync();

            return new BaseResult<RefundOrderDto, OrderDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.Accept<RefundOrderDto>(),
                StatusCode = StatusCodes.Status202Accepted,
                Response = null,
                Request = refundOrderDto
            };
        }

        public async Task<BaseResult<OrderQueryDto, Paginate<OrderDto>>> GetOrders(OrderQueryDto orderQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetOrders", orderQueryDto);

            var roles = GetAccountRolesFromJwt();

            if (roles[0].Equals(nameof(ERoleName.Organization)))
            {
                var referenceId = GetReferenceIdFromJwt();
                orderQueryDto.OrganizationId = referenceId;
            }

            var predicate = _unitOfWork.GetRepository<Order>()
                .BuildSearchPredicate(orderQueryDto.FilterQuery, orderQueryDto.FilterBy);

            if (orderQueryDto.Status is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.Status == orderQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.StartDate is not null && orderQueryDto.EndDate is not null)
            {
                var dateRangePredicate = _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(
                    orderQueryDto.StartDate,
                    orderQueryDto.EndDate
                );
                predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
            }

            if (orderQueryDto.OrderType is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.OrderType == orderQueryDto.OrderType;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.OrderCode is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.OrderCode == orderQueryDto.OrderCode;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.PaymentGateway is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.PaymentGateway == orderQueryDto.PaymentGateway;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.KioskId is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.KioskId == orderQueryDto.KioskId;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.OrganizationId is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.OrganizationId == orderQueryDto.OrganizationId;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.StoreId is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.StoreId == orderQueryDto.StoreId;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Order>()
                .BuildSortingQuery(orderQueryDto.SortBy, orderQueryDto.IsAsc);

            var orders = await _unitOfWork.GetRepository<Order>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: orderQueryDto.Page,
                size: orderQueryDto.Size,
                ignoreQueryFilter: true,
                include: x => x.Include(x => x.OrderDetails).Include(x => x.Payments)
                    .Include(x => x.Kiosk).ThenInclude(x => x.Store).ThenInclude(x => x.Organization)
            );


            var ordersDto = _mapper.Map<Paginate<OrderDto>>(orders, opt => { });

            LogMessage(LogLevel.Information, "Out GetOrders", ordersDto);

            return new BaseResult<OrderQueryDto, Paginate<OrderDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Order>(),
                Request = orderQueryDto,
                Response = ordersDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, OrderDto>> GetOrder(string orderId)
        {
            LogMessage(LogLevel.Information, "In GetOrder", orderId);

            var order = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(
                    predicate: x => x.OrderId == orderId,
                    include: x =>
                        x.Include(x => x.OrderDetails).Include(x => x.Payments).Include(x => x.Kiosk)
                            .ThenInclude(x => x.Store).ThenInclude(x => x.Organization)
                );


            if (order is null)
            {
                return new BaseResult<string, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Order>(),
                    Request = orderId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var orderDto = _mapper.Map<OrderDto>(order, opts => { });

            LogMessage(LogLevel.Information, "Out GetOrder", orderDto);

            return new BaseResult<string, OrderDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Order>(),
                Request = orderId,
                Response = orderDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, OrderDto>> ChangeOrderStatus(string orderId,
            ChangeOrderStatusDto changeOrderStatusDto)
        {
            LogMessage(LogLevel.Information, "In ChangeOrderStatus", orderId);

            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == orderId
            );

            if (order is null)
            {
                return new BaseResult<string, OrderDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Order>(),
                    Request = orderId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            switch (changeOrderStatusDto.Status)
            {
                case nameof(EOrderStatus.Pending):
                {
                    order.Pending();
                    break;
                }
                case nameof(EOrderStatus.Preparing):
                {
                    order.Preparing();
                    break;
                }
                case nameof(EOrderStatus.Completed):
                {
                    order.Completed();
                    break;
                }
                case nameof(EOrderStatus.Cancelled):
                {
                    order.Cancelled();
                    break;
                }
                case nameof(EOrderStatus.Failed):
                {
                    order.Failed();
                    break;
                }
            }

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.CommitAsync();

            var orderDto = _mapper.Map<OrderDto>(order);

            LogMessage(LogLevel.Information, "Out ChangeOrderStatus", orderDto);

            return new BaseResult<string, OrderDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Order>(),
                Request = orderId,
                Response = orderDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<OrderDetailQueryDto, Paginate<OrderDetailDto>>> GetOrderDetails(string orderId,
            OrderDetailQueryDto orderDetailQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetOrderDetails", orderId);

            var predicate = _unitOfWork.GetRepository<OrderDetail>()
                .BuildSearchPredicate(orderDetailQueryDto.FilterQuery, orderDetailQueryDto.FilterBy);

            Expression<Func<OrderDetail, bool>> orderIdFilter = x => x.OrderId == orderId;
            predicate = ExpressionHelper.CombineExpressions<OrderDetail>(predicate, orderIdFilter);

            var orderBy = _unitOfWork.GetRepository<OrderDetail>()
                .BuildSortingQuery(orderDetailQueryDto.SortBy, orderDetailQueryDto.IsAsc);

            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: orderDetailQueryDto.Page,
                size: orderDetailQueryDto.Size,
                include: null
            );

            var orderDetailsDto = _mapper.Map<Paginate<OrderDetailDto>>(orderDetails);

            LogMessage(LogLevel.Information, "Out GetOrderDetails", orderId);

            return new BaseResult<OrderDetailQueryDto, Paginate<OrderDetailDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<OrderDetail>(),
                StatusCode = StatusCodes.Status200OK,
                Response = orderDetailsDto,
                Request = orderDetailQueryDto
            };
        }

        public async Task<BaseResult<PaymentQueryDto, Paginate<PaymentDto>>> GetPayments(string orderId,
            PaymentQueryDto paymentQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetPayments", orderId);

            var predicate = _unitOfWork.GetRepository<Payment>()
                .BuildSearchPredicate(paymentQueryDto.FilterQuery, paymentQueryDto.FilterBy);

            Expression<Func<Payment, bool>> orderIdFilter = x => x.OrderId == orderId;
            predicate = ExpressionHelper.CombineExpressions<Payment>(predicate, orderIdFilter);

            var orderBy = _unitOfWork.GetRepository<Payment>()
                .BuildSortingQuery(paymentQueryDto.SortBy, paymentQueryDto.IsAsc);

            var payments = await _unitOfWork.GetRepository<Payment>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: paymentQueryDto.Page,
                size: paymentQueryDto.Size,
                include: null
            );

            var paymentDtos = _mapper.Map<Paginate<PaymentDto>>(payments);

            LogMessage(LogLevel.Information, "Out GetPayments", orderId);

            return new BaseResult<PaymentQueryDto, Paginate<PaymentDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<OrderDetail>(),
                StatusCode = StatusCodes.Status200OK,
                Response = paymentDtos,
                Request = paymentQueryDto
            };
        }

        public async Task<BaseResult<CancelMPOSQRResponse>> CancelMPOSQRPayment(string orderId)
        {
            //get order based on orderId
            var order = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x => x.OrderId.Equals(orderId));
            if (order is null)
                return new BaseResult<CancelMPOSQRResponse>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Order>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    ResponseRequest = new CancelMPOSQRResponse { OrderId = orderId }
                };

            if (order.Status!.Equals(nameof(EOrderStatus.Cancelled)))
                return new BaseResult<CancelMPOSQRResponse>()
                {
                    IsSuccess = false,
                    Message = "Order already canceled.",
                    StatusCode = StatusCodes.Status404NotFound,
                    ResponseRequest = new CancelMPOSQRResponse { OrderId = orderId }
                };
            //get payment and set to canceled
            var paymentBasedOrderId = await _unitOfWork.GetRepository<Payment>()
                .SingleOrDefaultAsync(predicate: x => x.OrderId.Equals(orderId));
            if (paymentBasedOrderId is null)
                return new BaseResult<CancelMPOSQRResponse>()
                {
                    IsSuccess = false,
                    Message = "Payment not found.",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ResponseRequest = new CancelMPOSQRResponse { OrderId = orderId }
                };
            paymentBasedOrderId.Cancelled();
            //update order status to cancelled
            order.Cancelled();
            //try cancel MPOS payment on cloud
            var mposCancelResponse = await _mPosClient.CancelQRPayment(orderId, order.FinalAmount.ToString() ?? "0");

            //check base on resCode in metadata
            if (mposCancelResponse.ResCode.Equals(MPOSStatusCode.Ok))
            {
                //success cancel on mpos cloud -> update commit
                await _unitOfWork.CommitAsync();
                return new BaseResult<CancelMPOSQRResponse>
                {
                    IsSuccess = true,
                    Message = "Order cancelled.",
                    StatusCode = StatusCodes.Status200OK,
                    ResponseRequest = new CancelMPOSQRResponse { OrderId = orderId }
                };
            }

            //if not success, return error message
            return new BaseResult<CancelMPOSQRResponse>
            {
                IsSuccess = false,
                Message = "Order cancelled but payment service failed.",
                StatusCode = StatusCodes.Status502BadGateway,
                ResponseRequest = new CancelMPOSQRResponse
                {
                    OrderId = orderId,
                    ResCode = mposCancelResponse.ResCode,
                    Message = mposCancelResponse.Message
                }
            };
        }

        public async Task<BaseResult<string, QRStatusResponse>> GetMPOSQRPaymentStatus(string orderId)
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x => x.OrderId.Equals(orderId));
            if (order is null)
                return new BaseResult<string, QRStatusResponse>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Order>(),
                    StatusCode = StatusCodes.Status404NotFound
                };
            var statusResponse = await _mPosClient.GetQRPaymentStatus(orderId, order.FinalAmount.ToString() ?? "0");
            return new BaseResult<string, QRStatusResponse>
            {
                IsSuccess = statusResponse.ResCode.Equals(MPOSStatusCode.Ok),
                Message = MessageUtil.ReadSuccess<Order>(),
                StatusCode = StatusCodes.Status200OK,
                Request = orderId,
                Response = new QRStatusResponse
                {
                    Status = statusResponse.TranStatusEnum,
                    OrderStatus = order.Status!,
                    Amount = statusResponse.Amount,
                    QrType = statusResponse.QrType,
                    TransDate = statusResponse.TransDate,
                    ResCode = statusResponse.ResCode,
                    Message = statusResponse.Message,
                }
            };
        }

        public async Task<MPOSCallbackResponse> HandleMPOSPaymentCallback(MPOSCallbackRequest request)
        {
            var transaction = _mPosClient.ParsePaymentCallbackRequest(request);
            await _capPublisher.PublishAsync(PaymentCapTopic.PaymentMPOSCallback, new PaymentMPOSCallbackMessage()
            {
                TranStatusEnum = transaction.TranStatusEnum.ToString(),
                TransStatus = transaction.TransStatus,
                OrderId = transaction.OrderId,
                PosId = transaction.PosId,
                Muid = transaction.Muid,
                IssuerCode = transaction.IssuerCode,
                ServiceName = transaction.ServiceName,
                TransAmount = transaction.TransAmount,
                TransDate = transaction.TransDate,
                TransCode = transaction.TransCode,
            });
            return new MPOSCallbackResponse()
            {
                Message = "Success",
                ResCode = 200
            };
        }

        public async Task<VNPAYCallbackResponse> HandleVNPAYPaymentCallback(VNPAYCallbackRequest request)
        {
            await _capPublisher.PublishAsync(PaymentCapTopic.PaymentVNPAYCallback, new PaymentVNPAYCallbackMessage()
            {
                TmnCode = request.TmnCode,
                Amount = request.Amount,
                BankCode = request.BankCode,
                BankTranNo = request.BankTranNo,
                CardType = request.CardType,
                PayDate = request.PayDate,
                PayDateParsed = request.PayDateParsed,
                OrderInfo = request.OrderInfo,
                TransactionNo = request.TransactionNo,
                ResponseCode = request.ResponseCode,
                TransactionStatus = request.TransactionStatus,
                TxnRef = request.TxnRef,
                SecureHash = request.SecureHash,
                TransactionStatusEnum = request.TransactionStatusEnum.ToString()
            });
            return VNPAYCallbackResponse.Success();
        }

        public async Task<BaseResult<object, object>> HandleOrderFailCallback(
            OrderKioskFailCallbackDto orderFailCallbackDto)
        {
            await _capPublisher.PublishAsync(OrderCapTopic.OrderKioskFailCallback,
                new OrderKioskFailCallbackCapMessage()
                {
                    OrderId = orderFailCallbackDto.OrderId,
                    Status = orderFailCallbackDto.Status,
                    Message = orderFailCallbackDto.Message,
                    FinishedProductIds = orderFailCallbackDto.FinishedProductIds,
                    FailedProductIds = orderFailCallbackDto.FailedProductIds,
                    PreparingProductIds = orderFailCallbackDto.PreparingProductIds
                });

            await _capPublisher.PublishAsync(NotificationCapTopic.NotificationOrder, new NotificationOrderCapMessage()
            {
                OrderId = orderFailCallbackDto.OrderId,
                CreatedBy = "System",
                NotificationType = ENotificationType.OrderExecuteFailed
            });

            return new BaseResult<object, object>()
            {
                IsSuccess = true,
                Message = "",
                StatusCode = StatusCodes.Status202Accepted,
                Response = null,
                Request = null
            };
        }

        public async Task<MemoryStream?> ExportOrder(OrderQueryDto orderQueryDto)
        {
            var roles = GetAccountRolesFromJwt();

            if (roles[0].Equals(nameof(ERoleName.Organization)))
            {
                var referenceId = GetReferenceIdFromJwt();
                orderQueryDto.OrganizationId = referenceId;
            }

            var predicate = _unitOfWork.GetRepository<Order>()
                .BuildSearchPredicate(orderQueryDto.FilterQuery, orderQueryDto.FilterBy);

            if (orderQueryDto.Status is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.Status == orderQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.OrderType is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.OrderType == orderQueryDto.OrderType;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.OrderCode is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.OrderCode == orderQueryDto.OrderCode;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.PaymentGateway is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.PaymentGateway == orderQueryDto.PaymentGateway;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.KioskId is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.KioskId == orderQueryDto.KioskId;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.OrganizationId is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.OrganizationId == orderQueryDto.OrganizationId;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }

            if (orderQueryDto.StoreId is not null)
            {
                Expression<Func<Order, bool>> statusFilter = x => x.StoreId == orderQueryDto.StoreId;
                predicate = ExpressionHelper.CombineExpressions<Order>(predicate, statusFilter);
            }


            var dateRangePredicate = _unitOfWork.GetRepository<Order>().BuildDateRangePredicate(
                orderQueryDto.StartDate,
                orderQueryDto.EndDate
            );

            predicate = ExpressionHelper.CombineExpressions<Order>(predicate, dateRangePredicate);


            var orderBy = _unitOfWork.GetRepository<Order>()
                .BuildSortingQuery(orderQueryDto.SortBy, orderQueryDto.IsAsc);

            var orders = await _unitOfWork.GetRepository<Order>().GetListAsync(
                predicate: predicate,
                orderBy: orderBy,
                include: x => x.Include(x => x.OrderDetails).Include(x => x.Payments)
            );

            var exportOrders = _mapper.Map<List<ExportOrderDto>>(orders);

            var ms = _memoryStreamManager.GetStream();

            ExcelExporter.ExportToStream(exportOrders, ms, "Orders", "Danh sách đơn hàng");

            ms.Position = 0;

            return ms;
        }

        /// <summary>
        /// Handle the state of order callback from kiosk
        /// </summary>
        /// <param name="orderCompleteCallbackDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<BaseResult<object, object>> HandleOrderCompleteCallback(
            OrderKioskCompleteCallbackDto orderCompleteCallbackDto)
        {
            await _capPublisher.PublishAsync(OrderCapTopic.OrderKioskCompleteCallback,
                new OrderKioskCompleteCallbackCapMessage()
                {
                    OrderId = orderCompleteCallbackDto.OrderId,
                    Status = orderCompleteCallbackDto.Status,
                    FinishedProductIdList = orderCompleteCallbackDto.FinishedProductIdList
                });

            return new BaseResult<object, object>()
            {
                IsSuccess = true,
                Message = "",
                StatusCode = StatusCodes.Status202Accepted,
                Response = null,
                Request = null
            };
        }

        private bool IsValidDiscountCode(string? input, out int discountPercent)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            discountPercent = 0;

            if (string.IsNullOrWhiteSpace(input) || input.Length <= 4 || !input.All(char.IsDigit))
                return false;

            try
            {
                // Parse DateTime: định dạng là HHmmddMMyy
                int hour = int.Parse(new string(input.Substring(0, 2).Reverse().ToArray()));
                int minute = int.Parse(new string(input.Substring(2, 2).Reverse().ToArray()));
                // int day = int.Parse(new string(input.Substring(4, 2).Reverse().ToArray()));
                // int month = int.Parse(new string(input.Substring(6, 2).Reverse().ToArray()));
                // int year = 2000 + int.Parse(new string(input.Substring(8, 2).Reverse().ToArray()));

                var parsed = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

                // Parse phần trăm giảm giá (phía sau 10 ký tự đầu)
                string discountPart = input.Substring(4);
                if (!int.TryParse(discountPart, out int percent) || percent < 0 || percent > 100)
                    return false;

                if (parsed == now)
                {
                    discountPercent = percent;
                    return true;
                }

                discountPercent = 0;
                return false;
            }
            catch
            {
                return false;
            }
        }


        // #region IngredientHelper
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // /// <param name="orderDetails"></param>
        // /// <param name="kioskDevices"></param>
        // /// <returns></returns>
        // private async Task<CheckIngredientsResult> CheckIngredientsAvailableAsync(
        //     List<OrderDetailNestedDto> orderDetails,
        //     List<KioskDeviceMapping> kioskDevices
        // )
        // {
        //     var result = new CheckIngredientsResult();
        //
        //     var productIds = orderDetails.Select(x => x.ProductId).ToList();
        //
        //     // B1: Lấy danh sách Product với ProductAttributes
        //     var products = new List<Product>();
        //     foreach (var productId in productIds)
        //     {
        //         var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //             predicate: x => x.ProductId == productId,
        //             include: x => x.Include(p => p.ProductAttributes!)
        //         );
        //
        //         if (product != null)
        //         {
        //             products.Add(product);
        //         }
        //     }
        //
        //     // B2: Tính tổng lượng nguyên liệu cần
        //     var ingredientNeeds = new Dictionary<string, double>();
        //     foreach (var orderDetail in orderDetails)
        //     {
        //         var product = products.FirstOrDefault(p => p.ProductId == orderDetail.ProductId);
        //         if (product?.ProductAttributes == null) continue;
        //
        //         foreach (var attr in product.ProductAttributes)
        //         {
        //             var amount = attr.DefaultAmount * orderDetail.Quantity;
        //             if (ingredientNeeds.ContainsKey(attr.IngredientType))
        //                 ingredientNeeds[attr.IngredientType] += amount;
        //             else
        //                 ingredientNeeds[attr.IngredientType] = amount;
        //         }
        //     }
        //
        //     // B3V2: Gom nguyên liệu từ thiết bị có IsPrimary = true
        //     var primaryIngredientSources = new Dictionary<string, List<DeviceIngredientState>>();
        //     foreach (var mapping in kioskDevices)
        //     {
        //         var device = mapping.Device;
        //         if (device?.DeviceIngredientStates == null) continue;
        //
        //         foreach (var state in device.DeviceIngredientStates)
        //         {
        //             if (state.IsWarning || !state.IsPrimary) continue;
        //
        //             var type = state.IngredientType;
        //
        //             if (!primaryIngredientSources.ContainsKey(type))
        //                 primaryIngredientSources[type] = new List<DeviceIngredientState>();
        //
        //             primaryIngredientSources[type].Add(state);
        //         }
        //     }
        //
        //     // B4V2: Kiểm tra và trừ nguyên liệu từ 1 thiết bị đủ riêng lẻ (không cộng dồn)
        //     foreach (var need in ingredientNeeds)
        //     {
        //         var type = need.Key;
        //         var required = need.Value;
        //
        //         if (!primaryIngredientSources.TryGetValue(type, out var sources) || sources.Count == 0)
        //         {
        //             result.MissingIngredients.Add(new MissingIngredientInfo
        //             {
        //                 IngredientType = type,
        //                 Required = required,
        //                 Available = 0,
        //                 DeviceNames = new List<string>()
        //             });
        //             continue;
        //         }
        //
        //         var availableDevice = sources.FirstOrDefault(s => s.CurrentCapacity >= required);
        //
        //         if (availableDevice == null)
        //         {
        //             result.MissingIngredients.Add(new MissingIngredientInfo
        //             {
        //                 IngredientType = type,
        //                 Required = required,
        //                 Available = sources.Sum(s => s.CurrentCapacity),
        //                 DeviceNames = sources.Select(s => s.Device?.Name ?? "(Unknown)").ToList()
        //             });
        //             continue;
        //         }
        //
        //         // Trước khi ApplyDelta
        //         var oldCapacity = availableDevice.CurrentCapacity;
        //
        //         availableDevice.ApplyDelta(-required);
        //         result.UpdatedStates.Add(availableDevice);
        //
        //         result.Histories.Add(new DeviceIngredientHistory
        //         {
        //             DeviceIngredientHistoryId = Guid.NewGuid().ToString(),
        //             DeviceIngredientStateId = availableDevice.DeviceIngredientStateId,
        //             DeltaAmount = -required,
        //             NewCapacity = availableDevice.CurrentCapacity,
        //             OldCapacity = oldCapacity,
        //             DeviceId = availableDevice.DeviceId,
        //             Action = EIngredientAction.Consumed.ToString(),
        //             PerformedBy = "System",
        //         });
        //     }
        //
        //     // Nếu có missing thì nghĩa là thiếu nguyên liệu
        //     result.IsSuccess = result.MissingIngredients.Count <= 0;
        //     return result;
        // }
        //
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // /// <param name="orderDetails"></param>
        // /// <param name="kioskDevices"></param>
        // private async Task<RestoreIngredientsResult> RestoreIngredientsFromOrderAsync(
        //     List<OrderDetail> orderDetails,
        //     List<KioskDeviceMapping> kioskDevices)
        // {
        //     var result = new RestoreIngredientsResult();
        //
        //     // B1: Lấy danh sách sản phẩm có ProductAttributes
        //     var productIds = orderDetails.Select(x => x.ProductId).ToList();
        //
        //     var products = new List<Product>();
        //     foreach (var productId in productIds)
        //     {
        //         var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
        //             predicate: x => x.ProductId == productId,
        //             include: x => x.Include(p => p.ProductAttributes!)
        //         );
        //
        //         if (product != null)
        //         {
        //             products.Add(product);
        //         }
        //     }
        //
        //     // B2: Tính lại lượng nguyên liệu cần "hoàn lại"
        //     var ingredientRestoreMap = new Dictionary<string, double>();
        //
        //     foreach (var orderDetail in orderDetails)
        //     {
        //         var product = products.FirstOrDefault(p => p.ProductId == orderDetail.ProductId);
        //         if (product?.ProductAttributes == null) continue;
        //
        //         foreach (var attr in product.ProductAttributes)
        //         {
        //             var amount = attr.DefaultAmount * orderDetail.Quantity;
        //             if (ingredientRestoreMap.ContainsKey(attr.IngredientType))
        //                 ingredientRestoreMap[attr.IngredientType] += amount;
        //             else
        //                 ingredientRestoreMap[attr.IngredientType] = amount;
        //         }
        //     }
        //
        //     // B3: Gom các thiết bị đang chứa nguyên liệu đó
        //     var ingredientSources = new Dictionary<string, List<DeviceIngredientState>>();
        //
        //     foreach (var mapping in kioskDevices)
        //     {
        //         var device = mapping.Device;
        //         if (device?.DeviceIngredientStates == null) continue;
        //
        //         foreach (var state in device.DeviceIngredientStates)
        //         {
        //             if (state.IsWarning || !state.IsPrimary) continue;
        //
        //             var type = state.IngredientType;
        //
        //             if (!ingredientSources.ContainsKey(type))
        //                 ingredientSources[type] = new List<DeviceIngredientState>();
        //
        //             ingredientSources[type].Add(state);
        //         }
        //     }
        //
        //     // B4: Cộng lại nguyên liệu vào thiết bị (ưu tiên thiết bị còn ít nhất)
        //     foreach (var restore in ingredientRestoreMap)
        //     {
        //         var type = restore.Key;
        //         var amountToRestore = restore.Value;
        //
        //         if (!ingredientSources.TryGetValue(type, out var states)) continue;
        //
        //         // Ưu tiên cộng vào thiết bị có lượng còn lại thấp nhất (tránh overflow nếu có MaxCapacity)
        //         foreach (var state in states.Where(x => x.IsPrimary).OrderBy(x => x.CurrentCapacity))
        //         {
        //             if (amountToRestore <= 0) break;
        //
        //             var oldCapacity = state.CurrentCapacity;
        //
        //             state.ApplyDelta(amountToRestore);
        //             result.UpdatedStates.Add(state);
        //
        //             result.Histories.Add(new DeviceIngredientHistory
        //             {
        //                 DeviceIngredientHistoryId = Guid.NewGuid().ToString(),
        //                 DeviceIngredientStateId = state.DeviceIngredientStateId,
        //                 DeltaAmount = amountToRestore,
        //                 DeviceId = state.DeviceId,
        //                 NewCapacity = state.CurrentCapacity,
        //                 OldCapacity = oldCapacity,
        //                 Action = EIngredientAction.Restore.ToString(),
        //                 PerformedBy = "System",
        //             });
        //
        //             break; // nếu chỉ cộng vào 1 thiết bị, bạn có thể bỏ break để chia đều
        //         }
        //     }
        //
        //     return result;
        // }
        //
        //
        // private class CheckIngredientsResult
        // {
        //     public bool IsSuccess { get; set; }
        //     public List<MissingIngredientInfo> MissingIngredients { get; set; } = new();
        //     public List<DeviceIngredientState> UpdatedStates { get; set; } = new(); // ← mới thêm
        //     public List<DeviceIngredientHistory> Histories { get; set; } = new(); // ← mới thêm
        // }
        //
        // private class RestoreIngredientsResult
        // {
        //     public List<DeviceIngredientState> UpdatedStates { get; set; } = new();
        //     public List<DeviceIngredientHistory> Histories { get; set; } = new();
        // }
        //
        //
        // public class MissingIngredientInfo
        // {
        //     public string IngredientType { get; set; } = default!;
        //     public double Required { get; set; }
        //     public double Available { get; set; }
        //     public List<string> DeviceNames { get; set; } = new();
        // }
        //
        // #endregion
    }
}
