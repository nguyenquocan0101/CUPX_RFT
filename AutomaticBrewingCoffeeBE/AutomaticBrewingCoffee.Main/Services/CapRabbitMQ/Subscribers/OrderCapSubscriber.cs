using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Services.CapRabbitMQ.Messages.Order;
using Services.CapRabbitMQ.Topics;
using Services.Dtos.Product;
using Services.SignalR.Services;
using Services.SignalR.Signal.Order;
using Services.Utils;

namespace Services.CapRabbitMQ.Subscribers;

public class OrderCapSubscriber : ICapSubscribe
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentCapSubscriber> _logger;
    private readonly TabletSignalService _tabletSignalService;

    public OrderCapSubscriber(
        IUnitOfWork unitOfWork,
        ILogger<PaymentCapSubscriber> logger,
        TabletSignalService tabletSignalService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _tabletSignalService = tabletSignalService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [CapSubscribe(OrderCapTopic.OrderKioskCompleteCallback)]
    public async Task HandleOrderKioskCompleteCallback(OrderKioskCompleteCallbackCapMessage message)
    {
        _logger.LogInformation($"HandleOrderKioskCallback");
        _logger.LogInformation($"OrderId: {message.OrderId}");
        _logger.LogInformation($"OrderStatus: {message.Status}");
        _logger.LogInformation($"OrderDetailsCompleted: {message.FinishedProductIdList.Count}");

        try
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == message.OrderId
            );

            if (order is null)
            {
                _logger.LogWarning("HandleOrderKioskCallback: Order not found");
                throw new NullReferenceException(
                    $"Order not found with OrderId = {message.OrderId} | OrderStatus = {message.Status}");
            }

            switch (message.Status)
            {
                case nameof(EOrderStatus.Pending):
                {
                    break;
                }
                case nameof(EOrderStatus.Preparing):
                {
                    break;
                }
                case nameof(EOrderStatus.Completed):
                {
                    order.Completed();

                    var productCompleteMakingDtos = await CreateProductMakingList(order, message.FinishedProductIdList);

                    order.CompletedProductIds = JsonConvert.SerializeObject(productCompleteMakingDtos);

                    break;
                }
                case nameof(EOrderStatus.Cancelled):
                {
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

            await _tabletSignalService.NotifyOrderAsync(order.ClientId, new OrderSignal()
            {
                OrderId = order.OrderId,
                OrderStatus = order.Status
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandleOrderKioskCallback");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [CapSubscribe(OrderCapTopic.OrderKioskFailCallback)]
    public async Task HandleOrderKioskFailCallback(OrderKioskFailCallbackCapMessage message)
    {
        _logger.LogInformation($"HandleOrderKioskCallback");
        _logger.LogInformation($"OrderId: {message.OrderId}");
        _logger.LogInformation($"OrderStatus: {message.Status}");

        try
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync(
                predicate: x => x.OrderId == message.OrderId
            );

            if (order is null)
            {
                _logger.LogWarning("HandleOrderKioskCallback: Order not found");
                throw new NullReferenceException(
                    $"Order not found with OrderId = {message.OrderId} | OrderStatus = {message.Status}");
            }

            switch (message.Status)
            {
                case nameof(EOrderStatus.Pending):
                {
                    break;
                }
                case nameof(EOrderStatus.Preparing):
                {
                    break;
                }
                case nameof(EOrderStatus.Completed):
                {
                    order.Completed();
                    break;
                }
                case nameof(EOrderStatus.Cancelled):
                {
                    break;
                }
                case nameof(EOrderStatus.Failed):
                {
                    order.Failed();

                    var productCompleteMakingDtos = await CreateProductMakingList(order, message.FinishedProductIds);
                    var productFailMakingDtos = await CreateProductMakingList(order, message.FailedProductIds);
                    var productPreparingMakingDtos = await CreateProductMakingList(order, message.PreparingProductIds);

                    order.CompletedProductIds = JsonConvert.SerializeObject(productCompleteMakingDtos);
                    order.FailedProductIds = JsonConvert.SerializeObject(productFailMakingDtos);
                    order.PreparingProductIds = JsonConvert.SerializeObject(productPreparingMakingDtos);

                    var kioskDevices = await _unitOfWork.GetRepository<KioskDeviceMapping>().GetListAsync(
                        predicate: x => x.KioskId == order.KioskId,
                        include: x => x.Include(x => x.Device).ThenInclude(x => x.DeviceIngredientStates)
                    );

                    var ingredientsToRestore =
                        await IngredientHelper.RestoreIngredientsFromOrderAsync(
                            _unitOfWork,
                            message.PreparingProductIds,
                            kioskDevices.ToList()
                        );

                    _unitOfWork.GetRepository<DeviceIngredientState>().UpdateRange(ingredientsToRestore.UpdatedStates);
                    await _unitOfWork.GetRepository<DeviceIngredientHistory>()
                        .InsertRangeAsync(ingredientsToRestore.Histories);
                    break;
                }
            }

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.CommitAsync();

            await _tabletSignalService.NotifyOrderAsync(order.ClientId, new OrderSignal()
            {
                OrderId = order.OrderId,
                OrderStatus = order.Status
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandleOrderKioskCallback");
            throw;
        }
    }

    #region Utils

    private async Task<List<ProductExecuteDto>> CreateProductMakingList(Order order, List<string> productIds)
    {
        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == order.KioskId
        );


        if (kiosk is null)
        {
            _logger.LogWarning("HandleOrderKioskCallback: Kiosk not found");
            throw new NullReferenceException(
                $"Kiosk not found with KioskId = {order.KioskId} | ProductIds = {productIds}");
        }

        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == kiosk.MenuId,
            include: x => x.Include(x => x.MenuProductMappings)
                .ThenInclude(x => x.Product)
        );

        if (menu is null)
        {
            _logger.LogWarning("HandleOrderKioskCallback: Menu not found");
            throw new NullReferenceException(
                $"Menu not found with MenuId = {kiosk.MenuId}");
        }

        var menuProducts =
            menu.MenuProductMappings?.Where(x => productIds.Contains(x.ProductId));

        if (menuProducts is null)
        {
            _logger.LogWarning("HandleOrderKioskCallback: MenuProducts not found");
            throw new NullReferenceException(
                "MenuProduct not found");
        }

        var productMakingDtos = menuProducts.Select(x => new ProductExecuteDto()
        {
            ProductId = x.ProductId,
            ProductName = x.Product.Name,
            SellingPrice = x.SellingPrice ?? x.Product.Price,
        }).ToList();

        return productMakingDtos;
    }

    // private async Task<RestoreIngredientsResult> RestoreIngredientsFromOrderAsync(
    //     List<string> productIds,
    //     List<KioskDeviceMapping> kioskDevices
    // )
    // {
    //     var result = new RestoreIngredientsResult();
    //
    //     // B1: Lấy danh sách sản phẩm có ProductAttributes
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
    //     foreach (var product in products)
    //     {
    //         if (product?.ProductAttributes == null) continue;
    //
    //         foreach (var attr in product.ProductAttributes)
    //         {
    //             var amount = attr.DefaultAmount;
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
    // private class RestoreIngredientsResult
    // {
    //     public List<DeviceIngredientState> UpdatedStates { get; set; } = new();
    //     public List<DeviceIngredientHistory> Histories { get; set; } = new();
    // }

    #endregion Utils
}