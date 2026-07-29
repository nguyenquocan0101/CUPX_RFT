using System.Text.Json;
using Services.Dtos.OrderCache;
using Services.ExternalClients;
using Services.Interfaces;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Services.CustomExceptions;

namespace Services.Implements
{
    public class OrderCacheService : IOrderCacheService
    {
        private readonly IDatabase _db;
        private readonly IMainBackendClient _mainBackendClient;
        private readonly ILogger<OrderCacheService> _logger;

        public OrderCacheService(IDatabase db, IMainBackendClient mainBackendClient, ILogger<OrderCacheService> logger)
        {
            _db = db;
            _mainBackendClient = mainBackendClient;
            _logger = logger;
        }

        public async Task<bool> AddAsync(string orderId, IEnumerable<string> productIdList)
        {
            try
            {
                var orderInCache = new CacheOrderDto(orderId, productIdList);
                var dataStr = JsonSerializer.Serialize(orderInCache);
                await _db.StringSetAsync(orderId, dataStr);
                _logger.LogInformation("Order {OrderId} cached with {ProductCount} products.", orderId, productIdList.Count());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<CacheOrderDto?> UpdateFinishProductInOrderAsync(string orderId, string productId, DateTime finishTime)
        {
            try
            {
                var orderInCacheStr = await _db.StringGetAsync(orderId);
                if (string.IsNullOrEmpty(orderInCacheStr))
                {
                    _logger.LogWarning("Order {OrderId} not found in cache.", orderId);
                    return null;
                }

                var orderInCache = JsonSerializer.Deserialize<CacheOrderDto>(orderInCacheStr);
                var product = orderInCache.Products.FirstOrDefault(p => p.ProductId == productId && p.FinishTime == null);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in order {OrderId}", productId, orderId);
                    return orderInCache;
                }

                product.FinishTime = finishTime;
                await _db.StringSetAsync(orderId, JsonSerializer.Serialize(orderInCache));

                _logger.LogInformation("Updated finish time of product {ProductId} in order {OrderId}", productId, orderId);

                return orderInCache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product {ProductId} in order {OrderId}", productId, orderId);
                return null;
            }
        }

        public async Task<CacheOrderDto?> UpdateFailProductInOrderAsync(string orderId, string productId, DateTime failTime)
        {
            try
            {
                var orderInCacheStr = await _db.StringGetAsync(orderId);
                if (string.IsNullOrEmpty(orderInCacheStr))
                {
                    _logger.LogWarning("Order {OrderId} not found in cache.", orderId);
                    return null;
                }

                var orderInCache = JsonSerializer.Deserialize<CacheOrderDto>(orderInCacheStr);
                var product = orderInCache.Products.FirstOrDefault(p => p.ProductId == productId && p.FinishTime == null);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found in order {OrderId}", productId, orderId);
                    return orderInCache;
                }
                orderInCache.IsFault = true;
                product.FailTime = failTime;
                await _db.StringSetAsync(orderId, JsonSerializer.Serialize(orderInCache));

                _logger.LogInformation("Updated fail time of product {ProductId} in order {OrderId}", productId, orderId);

                return orderInCache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product {ProductId} in order {OrderId}", productId, orderId);
                return null;
            }
        }

        public async Task<bool> UpdateCompleteOrderToCloudAsync(string orderId, List<string> finishedProductIdList)
        {
            try
            {
                var completeResult = await _mainBackendClient.CompleteOrderAsync(orderId, finishedProductIdList);
                if (completeResult)
                {
                    await _db.KeyDeleteAsync(orderId);
                    _logger.LogInformation("Order {OrderId} completed and removed from cache.", orderId);
                }
                else
                {
                    _logger.LogWarning("Order {OrderId} completion API returned false. Failed to update to cloud", orderId);
                }
                return completeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while checking completion of order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> IsOrderCompleteAsync(string orderId)
        {
            try
            {
                var orderInCacheStr = await _db.StringGetAsync(orderId);
                if (string.IsNullOrEmpty(orderInCacheStr))
                {
                    throw new NotFoundException($"order {orderId} not found in cache");
                }

                var orderInCache = JsonSerializer.Deserialize<CacheOrderDto>(orderInCacheStr);

                if (orderInCache.Products.Any(p => p.FinishTime == null))
                {
                    _logger.LogInformation("Order {OrderId} not complete yet. Waiting for all products finished.", orderId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while checking completion of order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> RemoveOrder(string orderId)
        {
            bool deleted = await _db.KeyDeleteAsync(orderId);

            return deleted;

        }

        public async Task<CacheOrderDto?> GetOrderbyIdAsync(string orderId)
        {
            try
            {
                var orderInCacheStr = await _db.StringGetAsync(orderId);
                if (string.IsNullOrEmpty(orderInCacheStr))
                {
                    _logger.LogWarning("Order {OrderId} not found in cache.", orderId);
                    return null;
                }

                var orderInCache = JsonSerializer.Deserialize<CacheOrderDto>(orderInCacheStr);
                return orderInCache;
            }
            catch (Exception)
            {
                _logger.LogWarning("Order {OrderId} not found in cache. Error", orderId);
                return null;
            }

        }

        public async Task<bool> UpdateFailedOrderToCloudAsync(string orderId, string message, List<string> finishedProductIds, List<string> failedProductIds, List<string> preparingProductIds)
        {
            try
            {
                var completeResult = await _mainBackendClient.FailOrderAsync(orderId, message, finishedProductIds, failedProductIds, preparingProductIds);
                if (completeResult)
                {
                    _logger.LogInformation("Update order {OrderId} to failed successful", orderId);
                }
                else
                {
                    _logger.LogWarning("Update failed order {OrderId} API returned false. Failed to update to cloud", orderId);
                }
                return completeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while updating failure of order {OrderId}", orderId);
                return false;
            }
        }
    }
}
