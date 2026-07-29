//using Services.Interfaces;
//using System.Text.Json;
//using AutoMapper;
//using Domain.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Repositories.Interfaces;
//using Services.Base;
//using Services.Dtos.Order;
//using Services.Utils;
//using Domain.Pagination;
//using System.Linq.Expressions;
//using Domain.Enums;
//using Services.ExternalClients;
//using Services.CustomExceptions;
//using Microsoft.EntityFrameworkCore;
//using MassTransit;
//using System.Text;
//using Microsoft.Extensions.Configuration;


//namespace Services.Implements
//{
//    public class OrderService : BaseService<OrderService>, IOrderService
//    {
//        private readonly CloudClient _cloudClient;
//        private readonly IPublishEndpoint _publishEndpoint;
//        private readonly IConfiguration _configuration;
//        public OrderService(
//            IUnitOfWork unitOfWork,
//            IMapper mapper,
//            ILoggerFactory loggerFactory,
//            IHttpContextAccessor httpContextAccessor,
//            CloudClient cloudClient,
//            IPublishEndpoint publishEndpoint,
//            IConfiguration configuration
//        ) : base(
//            unitOfWork,
//            mapper,
//            loggerFactory,
//            httpContextAccessor
//        )
//        {
//            _cloudClient = cloudClient;
//            _publishEndpoint = publishEndpoint;
//            _configuration = configuration;
//        }

//        public async Task<PrepareOrderDto> CreateOrder(CreateLocalOrderDto createOrderDto)
//        {
//            try
//            {
//                List<CloudOrderItemDto> orderDetails = new();
//                //get products
//                foreach (var createLocalOrderDetail in createOrderDto.CreateLocalOrderDetails)
//                {
//                    var product = await _unitOfWork.GetRepository<Product>()
//                        .SingleOrDefaultAsync(predicate: x => x.ProductId.Equals(createLocalOrderDetail.ProductId));
//                    if (product == null) throw new NotFoundException($"Product with id {createLocalOrderDetail.ProductId} not found");
//                    var localOrder = new CloudOrderItemDto()
//                    {
//                        ProductName = product.Name,
//                        ProductDescription = product.Description,
//                        SellingPrice = product.Price,
//                        Quantity = createLocalOrderDetail.Quantity
//                    };
//                    orderDetails.Add(localOrder);
//                }

//                //send request to cloud
//                var createRequest = new CloudOrderCreateRequest
//                {
//                    KioskId = _configuration["KioskId"]!,
//                    Content = "Thanh toán đơn hàng",
//                    OrderDetails = orderDetails
//                };

//                var orderPaymentInfo = await _cloudClient.GetOrderPaymentInfoAsync(createRequest, CreateOrderId());

//                var newLocalOrder = new LocalOrder
//                {
//                    OrderId = orderPaymentInfo.OrderId,
//                    OrderData = JsonSerializer.Serialize(orderPaymentInfo),
//                    Status = Enum.Parse<OrderStatus>(orderPaymentInfo.Status!),
//                    CreatedAt = DateTime.UtcNow,
//                    //TODO: hard code for now, consider to remove in future
//                    IsSynced = false,
//                };
//                await _unitOfWork.GetRepository<LocalOrder>().InsertAsync(newLocalOrder);

//                var localOrderDetails = new List<LocalOrderDetail>();
//                foreach (var orderDetail in orderPaymentInfo.OrderDetails)
//                {
//                    var localOrder = new LocalOrderDetail()
//                    {
//                        OrderDetailId = orderDetail.OrderDetailId,
//                        OrderId = orderDetail.OrderId,
//                        ProductName = orderDetail.ProductName,
//                        Quantity = orderDetail.Quantity,
//                        SellingPrice = orderDetail.SellingPrice,
//                        TotalAmount = orderDetail.TotalAmount,
//                        DetailData = JsonSerializer.Serialize(orderDetail),
//                        IsSynced = false
//                    };
//                    localOrderDetails.Add(localOrder);
//                }
//                await _unitOfWork.GetRepository<LocalOrderDetail>().InsertRangeAsync(localOrderDetails);

//                var orderDto = _mapper.Map<PrepareOrderDto>(orderPaymentInfo);
//                await _unitOfWork.CommitAsync();
//                return orderDto;
//            }
//            catch (Exception e)
//            {
//                throw new Exception("Create order failed", e);
//            }
//        }
//        private string CreateOrderId()
//        {
//            string pattern = "ORD";
//            StringBuilder builder = new();

//            builder.Append(pattern);

//            builder.Append(DateTime.UtcNow.ToString("yyyyMMdd"));

//            builder.Append(DateTime.UtcNow.ToString("HHmmss"));
//            return builder.ToString();
//        }

//        public async Task<BaseResult<OrderQueryDto, Paginate<LocalOrderDto>>> GetOrders(OrderQueryDto orderQueryDto)
//        {
//            LogMessage(LogLevel.Information, "In GetOrders", orderQueryDto);

//            Expression<Func<LocalOrder, bool>>? predicate = o => o.CreatedAt >= (orderQueryDto.FromDate ?? DateTime.MinValue) &&
//                                                                o.CreatedAt <= (orderQueryDto.ToDate ?? DateTime.MaxValue);

//            if (orderQueryDto.Status is not null)
//            {
//                Expression<Func<LocalOrder, bool>> statusFilter = x => x.Status == orderQueryDto.Status;
//                predicate = ExpressionHelper.CombineExpressions<LocalOrder>(predicate, statusFilter);
//            }


//            var orderBy = _unitOfWork.GetRepository<LocalOrder>()
//                .BuildSortingQuery(nameof(LocalOrder.CreatedAt), orderQueryDto.IsAsc);

//            var orders = await _unitOfWork.GetRepository<LocalOrder>().GetPagingListAsync(
//                predicate: predicate,
//                orderBy: orderBy,
//                page: orderQueryDto.Page,
//                size: orderQueryDto.Size,
//                include: q => q.Include(lo => lo.OrderDetails)
//            );

//            var ordersDto = _mapper.Map<Paginate<LocalOrderDto>>(orders);

//            LogMessage(LogLevel.Information, "Out GetOrders", ordersDto);

//            return new BaseResult<OrderQueryDto, Paginate<LocalOrderDto>>()
//            {
//                IsSuccess = true,
//                Message = "Orders found.",
//                Request = orderQueryDto,
//                Response = ordersDto,
//                StatusCode = StatusCodes.Status200OK
//            };
//        }

//        public async Task<BaseResult<string, LocalOrderDto>> GetOrder(string orderId)
//        {
//            LogMessage(LogLevel.Information, "In GetOrder", orderId);

//            var order = GetOrderWithOutBaseRsReturn(orderId);

//            if (order is null)
//            {
//                return new BaseResult<string, LocalOrderDto>()
//                {
//                    IsSuccess = false,
//                    Message = "Order not found.",
//                    Request = orderId,
//                    Response = null,
//                    StatusCode = StatusCodes.Status404NotFound
//                };
//            }

//            var orderDto = _mapper.Map<LocalOrderDto>(order);

//            LogMessage(LogLevel.Information, "Out GetOrder", orderDto);

//            return new BaseResult<string, LocalOrderDto>()
//            {
//                IsSuccess = true,
//                Message = "Order found.",
//                Request = orderId,
//                Response = orderDto,
//                StatusCode = StatusCodes.Status200OK
//            };
//        }

//        public LocalOrder? GetOrderWithOutBaseRsReturn(string orderId)
//        {
//            return _unitOfWork.GetRepository<LocalOrder>()
//                .SingleOrDefaultAsync(predicate: x => x.OrderId == orderId, include: q => q.Include(lo => lo.OrderDetails))
//                .Result;
//        }


//        public async Task<BaseResult<OrderDetailQueryDto, Paginate<LocalOrderDetailDto>>> GetOrderDetails(string orderId,
//            OrderDetailQueryDto orderDetailQueryDto)
//        {
//            LogMessage(LogLevel.Information, "In GetOrderDetails", orderId);

//            var predicate = _unitOfWork.GetRepository<LocalOrderDetail>()
//                .BuildSearchPredicate(orderDetailQueryDto.FilterQuery, orderDetailQueryDto.FilterBy);

//            Expression<Func<LocalOrderDetail, bool>> orderIdFilter = x => x.OrderId == orderId;
//            predicate = ExpressionHelper.CombineExpressions<LocalOrderDetail>(predicate, orderIdFilter);

//            var orderBy = _unitOfWork.GetRepository<LocalOrderDetail>()
//                .BuildSortingQuery(orderDetailQueryDto.SortBy, orderDetailQueryDto.IsAsc);

//            var orderDetails = await _unitOfWork.GetRepository<LocalOrderDetail>().GetPagingListAsync(
//                predicate: predicate,
//                orderBy: orderBy,
//                page: orderDetailQueryDto.Page,
//                size: orderDetailQueryDto.Size,
//                include: null
//            );

//            var orderDetailsDto = _mapper.Map<Paginate<LocalOrderDetailDto>>(orderDetails);

//            LogMessage(LogLevel.Information, "Out GetOrderDetails", orderId);

//            return new BaseResult<OrderDetailQueryDto, Paginate<LocalOrderDetailDto>>()
//            {
//                IsSuccess = true,
//                Message = "Get order details",
//                StatusCode = StatusCodes.Status200OK,
//                Response = orderDetailsDto,
//                Request = orderDetailQueryDto
//            };
//        }
//    }
//}