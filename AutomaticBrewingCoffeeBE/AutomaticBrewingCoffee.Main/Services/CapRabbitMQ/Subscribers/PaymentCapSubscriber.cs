using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Services.Utils;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Services.CapRabbitMQ.Messages.Notification;
using Services.CapRabbitMQ.Messages.Payment;
using Services.CapRabbitMQ.Topics;
using Services.Dtos.Order;
using Services.Dtos.ProductAttribute;
using Services.MPOS.Data;
using Services.SignalR.Services;
using Services.SignalR.Signal.Order;
using Services.SignalR.Signal.Payment;
using Services.Utils;
using Services.VNPay.Base;

namespace Services.CapRabbitMQ.Subscribers;

public class PaymentCapSubscriber : ICapSubscribe
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentCapSubscriber> _logger;
    private readonly TabletSignalService _tabletSignalService;
    private readonly ICapPublisher _capPublisher;


    public PaymentCapSubscriber(
        IUnitOfWork unitOfWork,
        ILoggerFactory loggerFactory,
        TabletSignalService tabletSignalService, ICapPublisher capPublisher)
    {
        _unitOfWork = unitOfWork;
        _tabletSignalService = tabletSignalService;
        _capPublisher = capPublisher;
        _logger = loggerFactory.CreateLogger<PaymentCapSubscriber>();
    }

    /// <summary>
    /// Callback handle for MPOS
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="NullReferenceException"></exception>
    [CapSubscribe(PaymentCapTopic.PaymentMPOSCallback)]
    public async Task HandlePaymentMPOSCallback(PaymentMPOSCallbackMessage message)
    {
        try
        {
            _logger.LogInformation($"HandlePaymentMPOSCallback");
            _logger.LogInformation($"OrderId: {message.OrderId}");
            _logger.LogInformation($"TranStatus: {message.TranStatusEnum}");

            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == message.OrderId,
                include: x => x.Include(x => x.OrderDetails)
            );

            if (order is null)
            {
                _logger.LogWarning("HandlePaymentMPOSCallback: Order not found");
                _logger.LogError(
                    $"Order not found with OrderId = {message.OrderId} | MPOSTransStatus = {message.TranStatusEnum}");
                return;
            }

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == order.KioskId,
                include: x => x.Include(x => x.KioskVersion)
                    .ThenInclude(x => x.KioskVersionProductMappings)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceModel)
                    .ThenInclude(x => x.DeviceIngredients)
            );

            if (kiosk is null)
            {
                _logger.LogWarning("HandlePaymentMPOSCallback: Kiosk not found");
                return;
            }

            var tabletInKiosk = kiosk.KioskDevices.FirstOrDefault(x => x.DeviceId == order.ClientId);

            if (tabletInKiosk is null)
            {
                _logger.LogWarning("Tablet in kiosk not found");
                return;
            }

            var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kiosk.KioskId && x.WebhookType == EWebhookType.ExecuteProduct.ToString()
            );

            if (webhook is null)
            {
                _logger.LogWarning("HandlePaymentMPOSCallback: Webhook not found");
                return;
            }

            var payment = new Payment()
            {
                PaymentId = Guid.NewGuid().ToString(),
                OrderId = order.OrderId,
                PaymentContent = null,
                ReferenceId = null,
                RequiredAmount = order.FinalAmount,
            };

            switch (message.TranStatusEnum)
            {
                case (nameof(MPOSTransStatus.Approved)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment approved");
                    order.Preparing();
                    payment.Success();
                    break;
                }
                case (nameof(MPOSTransStatus.Reversed)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment reversed");
                    order.Cancelled();
                    payment.Reversed();
                    break;
                }
                case (nameof(MPOSTransStatus.Settled)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment settled");
                    order.Preparing();
                    payment.Success();
                    break;
                }
                case (nameof(MPOSTransStatus.PendingSignature)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment pending signature");
                    order.Pending();
                    payment.Pending(DateTime.UtcNow.AddMinutes(2));
                    break;
                }
                case (nameof(MPOSTransStatus.Voided)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment voided");
                    order.Cancelled();
                    payment.Cancelled();
                    break;
                }
                case (nameof(MPOSTransStatus.Pending)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment pending");
                    order.Pending();
                    payment.Pending(DateTime.UtcNow.AddMinutes(2));
                    break;
                }
                case (nameof(MPOSTransStatus.Fail)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment fail");
                    order.Cancelled();
                    payment.Error();
                    break;
                }
                case (nameof(MPOSTransStatus.Refunded)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment refunded");
                    order.Cancelled();
                    payment.Refunded();
                    break;
                }
                case (nameof(MPOSTransStatus.Rejected)):
                {
                    _logger.LogInformation("HandlePaymentMPOSCallback: Payment rejected");
                    order.Cancelled();
                    payment.Expired();
                    break;
                }
                default:
                {
                    throw new InvalidOperationException(
                        $"HandlePaymentVNPAYCallback: Transaction status not support {message.TransStatus}");
                }
            }

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            await _unitOfWork.CommitAsync();

            await _tabletSignalService.NotifyPaymentAsync(order.ClientId, new PaymentSignal()
            {
                OrderId = payment.OrderId,
                PaymentStatus = payment.PaymentStatus,
                PaidAmount = payment.PaidAmount,
                PaymentId = payment.PaymentId,
                OrderStatus = order.Status
            });

            if (payment.PaymentStatus == EPaymentStatus.Success.ToString())
            {
                var products = await CreateOrderProductAsync(order, kiosk);

                var pushOrder = await NotifyKiosk(
                    tabletInKiosk.Side,
                    orderId: order.OrderId,
                    webhookUrl: webhook.WebhookUrl,
                    apiKey: kiosk.ApiKey!,
                    products: products
                );

                if (pushOrder is false)
                {
                    order.Failed();

                    await _tabletSignalService.NotifyOrderAsync(order.ClientId, new OrderSignal()
                    {
                        OrderId = order.OrderId,
                        OrderStatus = order.Status
                    });

                    var notificationKioskCapMessage = new NotificationKioskCapMessage()
                    {
                        KioskId = kiosk.KioskId,
                        CreatedBy = "System",
                        Delivery = order.OrderCode,
                        NotificationType = ENotificationType.KioskReceiveOrderFailed
                    };

                    await _capPublisher.PublishAsync(NotificationCapTopic.NotificationKiosk,
                        notificationKioskCapMessage);

                    var kioskDevices = await _unitOfWork.GetRepository<KioskDeviceMapping>().GetListAsync(
                        predicate: x => x.KioskId == kiosk.KioskId
                    );

                    var ingredientResult = await IngredientHelper.RestoreIngredientsFromOrderAsync(
                        _unitOfWork,
                        order.OrderId,
                        order.OrderDetails.ToList(),
                        kioskDevices.ToList()
                    );

                    _unitOfWork.GetRepository<DeviceIngredientState>().UpdateRange(ingredientResult.UpdatedStates);
                    await _unitOfWork.GetRepository<DeviceIngredientHistory>()
                        .InsertRangeAsync(ingredientResult.Histories);

                    _unitOfWork.GetRepository<Order>().Update(order);

                    await _unitOfWork.CommitAsync();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandlePaymentMPOSCallback");
            throw;
        }
    }

    /// <summary>
    /// Callback handle for VNPAY
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="NullReferenceException"></exception>
    [CapSubscribe(PaymentCapTopic.PaymentVNPAYCallback)]
    public async Task HandlePaymentVNPAYCallback(PaymentVNPAYCallbackMessage message)
    {
        try
        {
            _logger.LogInformation($"HandlePaymentVNPAYCallback");
            _logger.LogInformation($"OrderId: {message.OrderInfo}");
            _logger.LogInformation($"TranStatus: {message.TransactionStatusEnum}");

            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == message.OrderInfo,
                include: x => x.Include(x => x.OrderDetails)
            );

            if (order is null)
            {
                _logger.LogWarning("HandlePaymentVNPAYCallback: Order not found");
                throw new NullReferenceException(
                    $"Order not found with OrderId = {message.OrderInfo} | MPOSTransStatus = {message.TransactionStatusEnum}");
            }

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == order.KioskId,
                include: x => x.Include(x => x.KioskVersion)
                    .ThenInclude(x => x.KioskVersionProductMappings)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceModel)
                    .ThenInclude(x => x.DeviceIngredients)
            );

            if (kiosk is null)
            {
                _logger.LogWarning("HandlePaymentVNPAYCallback: Kiosk not found");
                throw new NullReferenceException($"Kiosk not found with KioskId = {order.KioskId}");
            }

            var tabletInKiosk = kiosk.KioskDevices.FirstOrDefault(x => x.DeviceId == order.ClientId);

            if (tabletInKiosk is null)
            {
                throw new NullReferenceException("Tablet in kiosk not found");
            }

            var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kiosk.KioskId && x.WebhookType == EWebhookType.ExecuteProduct.ToString()
            );

            if (webhook is null)
            {
                _logger.LogWarning("HandlePaymentVNPAYCallback: Webhook not found");
                throw new NullReferenceException("Webhook not found");
            }

            var payment = new Payment()
            {
                PaymentId = Guid.NewGuid().ToString(),
                OrderId = order.OrderId,
                PaymentContent = null,
                ReferenceId = null,
                RequiredAmount = order.FinalAmount,
            };

            switch (message.TransactionStatusEnum)
            {
                case (nameof(VNPayTransStatus.Success)):
                {
                    _logger.LogInformation("HandlePaymentVNPAYCallback: Payment success");
                    order.Preparing();
                    payment.Success();
                    break;
                }
                case (nameof(VNPayTransStatus.Failed)):
                {
                    _logger.LogInformation("HandlePaymentVNPAYCallback: Payment failed");
                    order.Cancelled();
                    payment.Failed();
                    break;
                }
                default:
                {
                    throw new InvalidOperationException(
                        $"HandlePaymentVNPAYCallback: Transaction status not support {message.TransactionStatus}");
                }
            }

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.GetRepository<Payment>().InsertAsync(payment);
            await _unitOfWork.CommitAsync();

            await _tabletSignalService.NotifyPaymentAsync(order.ClientId, new PaymentSignal()
            {
                OrderId = payment.OrderId,
                PaymentStatus = payment.PaymentStatus,
                PaidAmount = payment.PaidAmount,
                PaymentId = payment.PaymentId,
                OrderStatus = order.Status
            });

            if (payment.PaymentStatus == EPaymentStatus.Success.ToString())
            {
                var products = await CreateOrderProductAsync(order, kiosk);

                var pushOrder = await NotifyKiosk(
                    tabletInKiosk.Side,
                    orderId: order.OrderId,
                    webhookUrl: webhook.WebhookUrl,
                    apiKey: kiosk.ApiKey!,
                    products: products
                );

                if (pushOrder is false)
                {
                    order.Failed();

                    await _tabletSignalService.NotifyOrderAsync(order.ClientId, new OrderSignal()
                    {
                        OrderId = order.OrderId,
                        OrderStatus = order.Status
                    });

                    var notificationKioskCapMessage = new NotificationKioskCapMessage()
                    {
                        KioskId = kiosk.KioskId,
                        CreatedBy = "System",
                        Delivery = order.OrderCode,
                        NotificationType = ENotificationType.KioskReceiveOrderFailed
                    };

                    await _capPublisher.PublishAsync(NotificationCapTopic.NotificationKiosk,
                        notificationKioskCapMessage);

                    var kioskDevices = await _unitOfWork.GetRepository<KioskDeviceMapping>().GetListAsync(
                        predicate: x => x.KioskId == kiosk.KioskId
                    );

                    var ingredientResult = await IngredientHelper.RestoreIngredientsFromOrderAsync(
                        _unitOfWork,
                        order.OrderId,
                        order.OrderDetails.ToList(),
                        kioskDevices.ToList()
                    );

                    _unitOfWork.GetRepository<DeviceIngredientState>().UpdateRange(ingredientResult.UpdatedStates);
                    await _unitOfWork.GetRepository<DeviceIngredientHistory>()
                        .InsertRangeAsync(ingredientResult.Histories);

                    _unitOfWork.GetRepository<Order>().Update(order);
                    await _unitOfWork.CommitAsync();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandlePaymentVNPAYCallback");
            throw;
        }
    }

    #region Utils

    /// <summary>
    /// Using to push message to kiosk
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="products"></param>
    /// <param name="apiKey"></param>
    /// <param name="webhookUrl"></param>
    /// <param name="side">Notify kiosk know that order serve on left or right</param>
    /// <returns></returns>
    private async Task<bool> NotifyKiosk(string? side, string orderId, List<OrderProductDto> products, string apiKey,
        string webhookUrl
    )
    {
        var sideValue = side switch
        {
            nameof(ESide.Left) => 1,
            nameof(ESide.Right) => 2,
            _ => 0
        };

        _logger.LogWarning("Received webhook data with the following values:\n" +
                           "Side Value   : {SideValue}\n" +
                           "Order ID     : {OrderId}\n" +
                           "API Key      : {ApiKey}\n" +
                           "Webhook URL  : {WebhookUrl}\n" +
                           "Products     : {Products}",
            sideValue, orderId, apiKey, webhookUrl, JsonConvert.SerializeObject(products));

        var data = new OrderExecuteDto()
        {
            OrderId = orderId,
            Side = sideValue,
            Products = products
        };

        var result = await ApiUtil.PostAsync(
            webhookUrl,
            data,
            headers: new Dictionary<string, string>()
            {
                { "X-API-KEY", ApiKeyUtil.Decrypt(apiKey) }
            }
        );

        if (result.IsSuccessStatusCode)
        {
            _logger.LogInformation("HandlePaymentMPOSCallback: Notify Kiosk Success");
        }
        else
        {
            _logger.LogError("HandlePaymentMPOSCallback: Notify Kiosk Failed");
            _logger.LogError("Notify webhook data with the following values:\n" +
                             "Side Value   : {SideValue}\n" +
                             "Order ID     : {OrderId}\n" +
                             "API Key      : {ApiKey}\n" +
                             "Webhook URL  : {WebhookUrl}\n" +
                             "Products     : {Products} \n" +
                             "Data         : {Data}",
                sideValue, orderId, apiKey, webhookUrl, JsonConvert.SerializeObject(products),
                JsonConvert.SerializeObject(data));
        }

        return result.IsSuccessStatusCode;
    }

    /// <summary>
    /// Using to get the product variant to kioks base on parrent product
    /// </summary>
    /// <param name="order"></param>
    /// <param name="kiosk"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<List<OrderProductDto>> CreateOrderProductAsync(Order order, Kiosk kiosk)
    {
        // Bước 1: Tính tổng quantity theo từng Product cha
        var orderedParentQuantities = order.OrderDetails
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        // Bước 2: Tạo HashSet chứa các parentId cần tìm
        var orderedParentIds = new HashSet<string>(orderedParentQuantities.Keys);

        // Bước 3: Lấy danh sách productId trong Kiosk version
        var kioskProductIds = kiosk.KioskVersion!.KioskVersionProductMappings
            .Select(x => x.ProductId)
            .ToHashSet();

        // Bước 4: Lấy danh sách product con thỏa điều kiện
        var productMakings = await _unitOfWork.GetRepository<Product>().GetListAsync(
            predicate: x => x.ParentId != null
                            && orderedParentIds.Contains(x.ParentId)
                            && kioskProductIds.Contains(x.ProductId)
        );

        var orderProductDtos = new List<OrderProductDto>();

        foreach (var product in productMakings)
        {
            if (product.ParentId != null &&
                orderedParentQuantities.TryGetValue(product.ParentId, out var quantity))
            {
                // Tìm các OrderDetail tương ứng với parentId
                var matchedDetails = order.OrderDetails
                    .Where(x => x.ProductId == product.ParentId)
                    .ToList();

                // Map product attributes to option order for sending kiosk
                foreach (var detail in matchedDetails)
                {
                    var attributes = !string.IsNullOrEmpty(detail.ProductAttributes)
                        ? JsonConvert.DeserializeObject<List<ProductAttributeSelectDto>>(detail.ProductAttributes)
                        : new List<ProductAttributeSelectDto>();

                    for (int i = 0; i < detail.Quantity; i++)
                    {
                        var options = new List<OrderOptionDto>();

                        if (attributes != null)
                        {
                            foreach (var attribute in attributes)
                            {
                                var productAttribute = await _unitOfWork.GetRepository<ProductAttribute>()
                                    .SingleOrDefaultAsync(
                                        predicate: x => x.ProductAttributeId == attribute.ProductAttributeId,
                                        include: x => x.Include(x => x.AttributeOptions)
                                    );

                                if (productAttribute is null)
                                {
                                    _logger.LogWarning(
                                        $"CreateOrderProductAsync: Product to make has null productAttribute");
                                    continue;
                                }

                                if (productAttribute.AttributeOptions is null)
                                {
                                    _logger.LogWarning(
                                        $"CreateOrderProductAsync: Product to make has null productAttribute.AttributeOptions");
                                    continue;
                                }

                                var attributeOption = productAttribute.AttributeOptions.First(x =>
                                    x.AttributeOptionId == attribute.AttributeOptionId);

                                // Tìm thiết bị phù hợp với nguyên liệu
                                var kioskDevice = kiosk.KioskDevices.FirstOrDefault(x => x.Device != null &&
                                    x.Device.DeviceModel != null &&
                                    x.Device.DeviceModel.DeviceIngredients != null &&
                                    x.Device.DeviceModel.DeviceIngredients.Any(x =>
                                        x.IngredientType == productAttribute.IngredientType && x.IsPrimary
                                    )
                                );

                                if (kioskDevice == null)
                                {
                                    throw new Exception(
                                        $"Không tìm thấy thiết bị nào hỗ trợ nguyên liệu: {productAttribute.IngredientType}");
                                }

                                var applyDeviceModel = kioskDevice.Device!.DeviceModel!;


                                if (applyDeviceModel.DeviceIngredients != null)
                                {
                                    var applyTarget = applyDeviceModel.DeviceIngredients.FirstOrDefault(x =>
                                        x.IngredientType == productAttribute.IngredientType && x.IsPrimary
                                    );

                                    if (applyTarget != null && applyTarget.TargetOverrideParameter.IsNullOrEmpty())
                                    {
                                        continue;
                                    }

                                    var option = new OrderOptionDto()
                                    {
                                        Target = applyTarget?.TargetOverrideParameter,
                                        DeviceModelId = applyDeviceModel.DeviceModelId,
                                        Value = attributeOption.Value
                                    };

                                    options.Add(option);
                                }
                            }
                        }

                        orderProductDtos.Add(new OrderProductDto
                        {
                            ProductId = product.ProductId,
                            Options = options
                        });
                    }
                }
            }
        }

        if (!orderProductDtos.Any())
        {
            _logger.LogWarning($"CreateOrderProductAsync: Product to make is empty");
        }

        _logger.LogInformation($"CreateOrderProductAsync: Product to make count = {orderProductDtos.Count}");

        return orderProductDtos;
    }

    #endregion Utils
}