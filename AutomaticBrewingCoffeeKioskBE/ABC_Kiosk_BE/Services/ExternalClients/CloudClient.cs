using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Services.ExternalClients;

public sealed class CloudClient(HttpClient httpClient, IConfiguration configuration, ILogger<CloudClient> logger)
{
    public async Task<bool> CompleteOrderAsync(string orderId, List<string> finishedProductIdList)
    {
        try
        {
            var json = JsonSerializer.Serialize(new CompleteOrderRequest(orderId, finishedProductIdList));
            Console.WriteLine(json.ToString());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-API-Key", configuration["ApiKey"]);
            logger.LogInformation($"Call cloud api to complete order {orderId}");
            var url = $"{httpClient.BaseAddress}{configuration["CloudConfig:OrdersEndpoint"]}/complete";
            var response = await httpClient.PutAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync()!;
            Console.WriteLine(responseString);
            using var document = JsonDocument.Parse(responseString);
            var root = document.RootElement;
            if (root.TryGetProperty("isSuccess", out var responseElement))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var rs =  JsonSerializer.Deserialize<bool>(responseElement, options);
                logger.LogInformation($"Success to complete order {orderId} with Cloud");
                return rs;
            }

            return false;
        }
        catch (Exception)
        {
            logger.LogError("Failed to complete order with cloud");
            return false;
        }
    }

    public async Task<bool> FailOrderAsync(string orderId, string message, List<string> finishedProductIds, List<string> failedProductIds, List<string> preparingProductIds)
    {
        try
        {
            var json = JsonSerializer.Serialize(new FailOrderRequest(orderId, message, finishedProductIds, failedProductIds, preparingProductIds));
            Console.WriteLine(json.ToString());
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("X-API-Key", configuration["ApiKey"]);
            logger.LogInformation($"Call cloud api to update order {orderId} to fail status");
            var url = $"{httpClient.BaseAddress}{configuration["CloudConfig:OrdersEndpoint"]}/fail";
            var response = await httpClient.PutAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync()!;
            Console.WriteLine(responseString);
            using var document = JsonDocument.Parse(responseString);
            var root = document.RootElement;
            if (root.TryGetProperty("isSuccess", out var responseElement))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var rs = JsonSerializer.Deserialize<bool>(responseElement, options);
                logger.LogInformation($"Success to update order {orderId} to Failed status with Cloud");
                return rs;
            }

            return false;
        }
        catch (Exception)
        {
            logger.LogError("Failed to complete order with cloud");
            throw new Exception("Interact with Cloud failed");
        }
    }


}

#region Request Class

public class  CompleteOrderRequest 
{
    public CompleteOrderRequest(string orderId, List<string> finishedProductIdList)
    {
        OrderId = orderId;
        Status = "Completed";
        FinishedProductIdList = finishedProductIdList;
    }
    public string OrderId { get; set; }
    public string Status { get; set; }
    public List<string> FinishedProductIdList { get; set; }
}


public class FailOrderRequest
{
    public FailOrderRequest(string orderId, string message, List<string> finishedProductIds, List<string> failedProductIds, List<string> preparingProductIds)
    {
        OrderId = orderId;
        Status = "Failed";
        Message = message;
        FinishedProductIds = finishedProductIds;
        FailedProductIds = failedProductIds;
        PreparingProductIds = preparingProductIds;
    }

    public string OrderId { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public List<string> FinishedProductIds { get; set; }
    public List<string> FailedProductIds { get; set; }
    public List<string> PreparingProductIds { get; set; }

}

#endregion


#region Response Class

#endregion

