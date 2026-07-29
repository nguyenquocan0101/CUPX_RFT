using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Services.BackgroundJobs;

public class PaymentExpiredJob(IUnitOfWork unitOfWork, ILogger<PaymentExpiredJob> logger)
{
    public async Task ExpirePaymentAutomatically()
    {
        logger.LogInformation("PaymentExpiredJob started at {Time}", DateTime.UtcNow);
        await CheckPaymentExpired();
        logger.LogInformation("PaymentExpiredJob finished at {Time}", DateTime.UtcNow);
    }

    private async Task CheckPaymentExpired()
    {
        logger.LogInformation("Checking for expired payments...");

        var minAgo = DateTime.UtcNow.AddHours(-3);

        var orders = await unitOfWork.GetRepository<Order>()
            .GetListAsync(
                predicate: x => x.Status == nameof(EOrderStatus.Pending) && x.CreatedDate >= minAgo,
                include: x => x.Include(x => x.OrderDetails)
            );

        foreach (var order in orders)
        {
            var payment = await unitOfWork.GetRepository<Payment>()
                .SingleOrDefaultAsync(predicate: x => x.PaymentStatus == EPaymentStatus.Pending.ToString());

            if (payment is null)
            {
                logger.LogWarning("Payment not found for expired payment with orderId = {PaymentId}", order.OrderId);
                continue;
            }

            if (!payment.CheckExpired())
            {
                continue;
            }

            logger.LogInformation("Cancelled order {OrderId} due to expired payment", order.OrderId);
            order.Cancelled("Cancelled by system because its payment expired");

            var newPayment = new Payment()
            {
                PaymentId = Guid.NewGuid().ToString(),
                OrderId = order.OrderId,
                PaymentContent = "Expired",
                ReferenceId = null,
                CreateBy = "Expired automatically by system",
                RequiredAmount = order.FinalAmount,
            };

            newPayment.Expired();

            logger.LogInformation("Marked payment {PaymentId} as expired", newPayment.PaymentId);

            var kiosk = await unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == order.KioskId,
                include: x => x.Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
            );

            var ingredientsToRestore =
                await RestoreIngredientsFromOrderAsync(order.OrderDetails.ToList(), kiosk.KioskDevices.ToList());

            kiosk = null;

            unitOfWork.GetRepository<DeviceIngredientState>().UpdateRange(ingredientsToRestore.UpdatedStates);
            await unitOfWork.GetRepository<DeviceIngredientHistory>().InsertRangeAsync(ingredientsToRestore.Histories);
            await unitOfWork.GetRepository<Payment>().InsertAsync(newPayment);
            unitOfWork.GetRepository<Order>().Update(order);

            await unitOfWork.CommitAsync();
            unitOfWork.ClearTracking();
        }
    }

    private async Task<RestoreIngredientsResult> RestoreIngredientsFromOrderAsync(
        List<OrderDetail> orderDetails,
        List<KioskDeviceMapping> kioskDevices)
    {
        var result = new RestoreIngredientsResult();

        // B1: Lấy danh sách sản phẩm có ProductAttributes
        var productIds = orderDetails.Select(x => x.ProductId).ToList();

        var products = new List<Product>();
        foreach (var productId in productIds)
        {
            var product = await unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
                predicate: x => x.ProductId == productId,
                include: x => x.Include(p => p.ProductAttributes!)
            );

            if (product != null)
            {
                products.Add(product);
            }
        }

        // B2: Tính lại lượng nguyên liệu cần "hoàn lại"
        var ingredientRestoreMap = new Dictionary<string, double>();

        foreach (var orderDetail in orderDetails)
        {
            var product = products.FirstOrDefault(p => p.ProductId == orderDetail.ProductId);
            if (product?.ProductAttributes == null) continue;

            foreach (var attr in product.ProductAttributes)
            {
                var amount = attr.DefaultAmount * orderDetail.Quantity;
                if (ingredientRestoreMap.ContainsKey(attr.IngredientType))
                    ingredientRestoreMap[attr.IngredientType] += amount;
                else
                    ingredientRestoreMap[attr.IngredientType] = amount;
            }
        }

        // B3: Gom các thiết bị đang chứa nguyên liệu đó
        var ingredientSources = new Dictionary<string, List<DeviceIngredientState>>();

        foreach (var mapping in kioskDevices)
        {
            var device = mapping.Device;
            if (device?.DeviceIngredientStates == null) continue;

            foreach (var state in device.DeviceIngredientStates)
            {
                if (state.IsWarning || !state.IsPrimary) continue;

                var type = state.IngredientType;

                if (!ingredientSources.ContainsKey(type))
                    ingredientSources[type] = new List<DeviceIngredientState>();

                ingredientSources[type].Add(state);
            }
        }

        // B4: Cộng lại nguyên liệu vào thiết bị (ưu tiên thiết bị còn ít nhất)
        foreach (var restore in ingredientRestoreMap)
        {
            var type = restore.Key;
            var amountToRestore = restore.Value;

            if (!ingredientSources.TryGetValue(type, out var states)) continue;

            // Ưu tiên cộng vào thiết bị có lượng còn lại thấp nhất (tránh overflow nếu có MaxCapacity)
            foreach (var state in states.Where(x => x.IsPrimary).OrderBy(x => x.CurrentCapacity))
            {
                if (amountToRestore <= 0) break;

                var oldCapacity = state.CurrentCapacity;

                state.ApplyDelta(amountToRestore);
                result.UpdatedStates.Add(state);

                result.Histories.Add(new DeviceIngredientHistory
                {
                    DeviceIngredientHistoryId = Guid.NewGuid().ToString(),
                    DeviceIngredientStateId = state.DeviceIngredientStateId,
                    DeltaAmount = amountToRestore,
                    DeviceId = state.DeviceId,
                    NewCapacity = state.CurrentCapacity,
                    OldCapacity = oldCapacity,
                    Action = EIngredientAction.Restore.ToString(),
                    PerformedBy = "System",
                });

                break; // nếu chỉ cộng vào 1 thiết bị, bạn có thể bỏ break để chia đều
            }
        }

        return result;
    }

    private class RestoreIngredientsResult
    {
        public List<DeviceIngredientState> UpdatedStates { get; set; } = new();
        public List<DeviceIngredientHistory> Histories { get; set; } = new();
    }
}