using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AutomaticBrewingCoffee.API.Health;

public sealed class RabbitMqHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["RabbitMQ:HostName"] ?? "127.0.0.1";
        var port = configuration.GetValue("RabbitMQ:Port", 5672);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy("RabbitMQ is unavailable.");
        }
    }
}
