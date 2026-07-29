using AutomaticBrewingCoffee.Domain.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AutomaticBrewingCoffee.API.Health;

public sealed class SqlServerHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutoBrewingBeContext>();

        return await context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("SQL Server is unavailable.");
    }
}
