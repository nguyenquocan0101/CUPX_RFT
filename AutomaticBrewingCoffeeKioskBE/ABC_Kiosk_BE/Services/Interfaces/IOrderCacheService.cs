
using Services.Dtos.OrderCache;

namespace Services.Interfaces
{
    public interface IOrderCacheService
    {
        /// <summary>
        /// Add a new order with its products to the cache for checking order completion later.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task<bool> AddAsync(string orderId, IEnumerable<string> productIdList);
        /// <summary>
        /// Update the finish time of a product in an order after 1 workflow of product DONE
        /// Message will be sent from WorkflowObserverWorker
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <param name="finishTime"></param>
        /// <returns></returns>
        Task<CacheOrderDto> UpdateFinishProductInOrderAsync(string orderId, string productId, DateTime finishTime);

        /// <summary>
        /// Update the fail time of a product in an order 
        /// Message will be sent from WorkflowObserverWorker
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <param name="failTime"></param>
        /// <returns></returns>
        Task<CacheOrderDto?> UpdateFailProductInOrderAsync(string orderId, string productId, DateTime failTime);
        /// <summary>
        /// Update complete the order to complete in cloud database
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> UpdateCompleteOrderToCloudAsync(string orderId, List<string> finishedProductIdList);

        /// <summary>
        /// Update failed order to cloud 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> UpdateFailedOrderToCloudAsync(string orderId, string message, List<string> finishedProductIds, List<string> failedProductIds, List<string> preparingProductIds);

        /// <summary>
        /// Remove order in redis
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> RemoveOrder(string orderId);


        /// <summary>
        /// get order in redis
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<CacheOrderDto?> GetOrderbyIdAsync(string orderId);

        /// <summary>
        /// Check whether order is complete or not based on product which have finish time
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> IsOrderCompleteAsync(string orderId);
    }
}
