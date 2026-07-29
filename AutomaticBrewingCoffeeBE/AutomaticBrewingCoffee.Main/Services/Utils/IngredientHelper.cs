using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.Dtos.OrderDetail;

namespace Services.Utils
{
    public static class IngredientHelper
    {
        public class CheckIngredientsResult
        {
            public bool IsSuccess { get; set; }
            public List<MissingIngredientInfo> MissingIngredients { get; set; } = new();
            public List<DeviceIngredientState> UpdatedStates { get; set; } = new();
            public List<DeviceIngredientHistory> Histories { get; set; } = new();
        }

        public class RestoreIngredientsResult
        {
            public List<DeviceIngredientState> UpdatedStates { get; set; } = new();
            public List<DeviceIngredientHistory> Histories { get; set; } = new();
        }

        public class MissingIngredientInfo
        {
            public string IngredientType { get; set; } = default!;
            public double Required { get; set; }
            public double Available { get; set; }
            public List<string> DeviceNames { get; set; } = new();
        }

        /// <summary>
        /// Kiểm tra nguyên liệu đủ hay không và trừ ngay nếu đủ.
        /// </summary>
        public static async Task<CheckIngredientsResult> CheckIngredientsAvailableAsync(
            IUnitOfWork unitOfWork,
            string orderId,
            List<OrderDetailNestedDto> orderDetails,
            List<KioskDeviceMapping> kioskDevices
        )
        {
            var result = new CheckIngredientsResult();

            var productIds = orderDetails.Select(x => x.ProductId).ToList();

            // B1: Lấy danh sách Product với ProductAttributes
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

            // B2: Tính tổng lượng nguyên liệu cần
            var ingredientNeeds = new Dictionary<string, double>();
            foreach (var orderDetail in orderDetails)
            {
                var product = products.FirstOrDefault(p => p.ProductId == orderDetail.ProductId);
                if (product?.ProductAttributes == null) continue;

                foreach (var attr in product.ProductAttributes)
                {
                    var amount = attr.DefaultAmount * orderDetail.Quantity;
                    if (ingredientNeeds.ContainsKey(attr.IngredientType))
                        ingredientNeeds[attr.IngredientType] += amount;
                    else
                        ingredientNeeds[attr.IngredientType] = amount;
                }
            }

            // B3V2: Gom nguyên liệu từ thiết bị có IsPrimary = true
            var primaryIngredientSources = new Dictionary<string, List<DeviceIngredientState>>();
            foreach (var mapping in kioskDevices)
            {
                var device = mapping.Device;
                if (device?.DeviceIngredientStates == null) continue;

                foreach (var state in device.DeviceIngredientStates)
                {
                    if (state.IsWarning || !state.IsPrimary) continue;

                    var type = state.IngredientType;

                    if (!primaryIngredientSources.ContainsKey(type))
                        primaryIngredientSources[type] = new List<DeviceIngredientState>();

                    primaryIngredientSources[type].Add(state);
                }
            }

            // B4V2: Kiểm tra và trừ nguyên liệu từ 1 thiết bị đủ riêng lẻ (không cộng dồn)
            foreach (var need in ingredientNeeds)
            {
                var type = need.Key;
                var required = need.Value;

                if (!primaryIngredientSources.TryGetValue(type, out var sources) || sources.Count == 0)
                {
                    result.MissingIngredients.Add(new MissingIngredientInfo
                    {
                        IngredientType = type,
                        Required = required,
                        Available = 0,
                        DeviceNames = new List<string>()
                    });
                    continue;
                }

                var availableDevice = sources.FirstOrDefault(s => s.CurrentCapacity >= required);

                if (availableDevice == null)
                {
                    result.MissingIngredients.Add(new MissingIngredientInfo
                    {
                        IngredientType = type,
                        Required = required,
                        Available = sources.Sum(s => s.CurrentCapacity),
                        DeviceNames = sources.Select(s => s.Device?.Name ?? "(Unknown)").ToList()
                    });
                    continue;
                }

                // Trước khi ApplyDelta
                var oldCapacity = availableDevice.CurrentCapacity;

                availableDevice.ApplyDelta(-required);
                result.UpdatedStates.Add(availableDevice);

                result.Histories.Add(new DeviceIngredientHistory
                {
                    DeviceIngredientHistoryId = Guid.NewGuid().ToString(),
                    DeviceIngredientStateId = availableDevice.DeviceIngredientStateId,
                    DeltaAmount = -required,
                    IngredientType = availableDevice.IngredientType,
                    NewCapacity = availableDevice.CurrentCapacity,
                    OldCapacity = oldCapacity,
                    DeviceId = availableDevice.DeviceId,
                    Action = EIngredientAction.Consumed.ToString(),
                    PerformedBy = "System",
                    OrderId = orderId
                });
            }

            // Nếu có missing thì nghĩa là thiếu nguyên liệu
            result.IsSuccess = result.MissingIngredients.Count <= 0;
            return result;
        }

        /// <summary>
        /// Hoàn lại nguyên liệu khi huỷ đơn.
        /// </summary>
        public static async Task<RestoreIngredientsResult> RestoreIngredientsFromOrderAsync(
            IUnitOfWork unitOfWork,
            string orderId,
            List<OrderDetail> orderDetails,
            List<KioskDeviceMapping> kioskDevices
        )
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
                    if (!state.IsPrimary) continue;

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
                        IngredientType = state.IngredientType,
                        NewCapacity = state.CurrentCapacity,
                        OldCapacity = oldCapacity,
                        Action = EIngredientAction.Restore.ToString(),
                        PerformedBy = "System",
                        OrderId = orderId
                    });

                    break; // nếu chỉ cộng vào 1 thiết bị, bạn có thể bỏ break để chia đều
                }
            }

            return result;
        }

        public static async Task<RestoreIngredientsResult> RestoreIngredientsFromOrderAsync(
            IUnitOfWork unitOfWork,
            List<string> productIds,
            List<KioskDeviceMapping> kioskDevices
        )
        {
            var result = new RestoreIngredientsResult();

            // B1: Lấy danh sách sản phẩm có ProductAttributes

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

            foreach (var product in products)
            {
                if (product?.ProductAttributes == null) continue;

                foreach (var attr in product.ProductAttributes)
                {
                    var amount = attr.DefaultAmount;
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
                        IngredientType = state.IngredientType,
                        OldCapacity = oldCapacity,
                        Action = EIngredientAction.Restore.ToString(),
                        PerformedBy = "System",
                    });

                    break; // nếu chỉ cộng vào 1 thiết bị, bạn có thể bỏ break để chia đều
                }
            }

            return result;
        }
    }
}