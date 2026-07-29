namespace Services.ExternalClients;

public interface IMainBackendClient
{
    Task<bool> CompleteOrderAsync(string orderId, List<string> finishedProductIdList);

    Task<bool> FailOrderAsync(
        string orderId,
        string message,
        List<string> finishedProductIds,
        List<string> failedProductIds,
        List<string> preparingProductIds);
}
