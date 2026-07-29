using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Services.ExternalClients;

public sealed class MainBackendOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5100";
    public string OrdersEndpoint { get; set; } = "/api/v1/orders";
    public string OutboundApiKey { get; set; } = string.Empty;
}

public sealed class MainBackendClient : IMainBackendClient
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly HttpClient _httpClient;
    private readonly MainBackendOptions _options;
    private readonly ILogger<MainBackendClient> _logger;

    public MainBackendClient(
        HttpClient httpClient,
        IOptions<MainBackendOptions> options,
        ILogger<MainBackendClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl, UriKind.Absolute);
        }
    }

    public Task<bool> CompleteOrderAsync(string orderId, List<string> finishedProductIdList)
    {
        return SendOrderUpdateAsync(
            "complete",
            new CompleteOrderRequest(orderId, finishedProductIdList));
    }

    public Task<bool> FailOrderAsync(
        string orderId,
        string message,
        List<string> finishedProductIds,
        List<string> failedProductIds,
        List<string> preparingProductIds)
    {
        return SendOrderUpdateAsync(
            "fail",
            new FailOrderRequest(orderId, message, finishedProductIds, failedProductIds, preparingProductIds));
    }

    private async Task<bool> SendOrderUpdateAsync(string action, object request)
    {
        try
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Put,
                BuildEndpoint(action));
            httpRequest.Content = JsonContent.Create(request);

            if (!string.IsNullOrWhiteSpace(_options.OutboundApiKey))
            {
                httpRequest.Headers.TryAddWithoutValidation(ApiKeyHeaderName, _options.OutboundApiKey);
            }

            using var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Main backend order update returned HTTP {StatusCode} for action {Action}.", response.StatusCode, action);
                return false;
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(responseStream);
            return document.RootElement.TryGetProperty("isSuccess", out var isSuccess)
                && isSuccess.ValueKind == JsonValueKind.True;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Main backend order update failed for action {Action}.", action);
            return false;
        }
    }

    private string BuildEndpoint(string action)
    {
        var endpoint = _options.OrdersEndpoint.TrimEnd('/');
        return $"{endpoint}/{action}";
    }
}

public sealed record CompleteOrderRequest(string OrderId, List<string> FinishedProductIdList)
{
    public string Status { get; init; } = "Completed";
}

public sealed record FailOrderRequest(
    string OrderId,
    string Message,
    List<string> FinishedProductIds,
    List<string> FailedProductIds,
    List<string> PreparingProductIds)
{
    public string Status { get; init; } = "Failed";
}
