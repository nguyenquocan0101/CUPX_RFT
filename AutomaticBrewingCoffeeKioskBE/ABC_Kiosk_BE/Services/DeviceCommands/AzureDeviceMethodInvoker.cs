using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using Shared.MessageStore;

namespace Services.DeviceCommands;

public sealed class AzureDeviceMethodInvoker : IDeviceMethodInvoker, IDisposable
{
    private readonly ServiceClient _serviceClient;

    public AzureDeviceMethodInvoker(IConfiguration configuration)
    {
        var connectionString = configuration["AzureServiceConn"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("AzureDeviceMethodInvoker requires AzureServiceConn.");
        _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
    }

    public async Task<DeviceCommandResult> InvokeAsync(
        DeviceCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var method = new CloudToDeviceMethod(
            request.Method,
            responseTimeout: TimeSpan.FromMilliseconds(Math.Clamp(request.TimeoutMs, 100, 30000)),
            connectionTimeout: TimeSpan.FromSeconds(5));
        if (request.Parameters.TryGetValue("raw", out var raw) && !string.IsNullOrWhiteSpace(raw))
            method.SetPayloadJson(raw);

        var response = await _serviceClient.InvokeDeviceMethodAsync(request.DeviceId, method, cancellationToken);
        return new DeviceCommandResult(
            request.CommandId,
            request.SchemaVersion,
            request.CorrelationId,
            request.DeviceId,
            response.Status == 200 ? "Completed" : "Failed",
            new Dictionary<string, string> { ["status"] = response.Status.ToString(), ["payload"] = response.GetPayloadAsJson() },
            response.Status == 200 ? null : "AZURE_DEVICE_FAILURE",
            response.Status == 200 ? null : response.GetPayloadAsJson(),
            DateTimeOffset.UtcNow);
    }

    public void Dispose() => _serviceClient.Dispose();
}
