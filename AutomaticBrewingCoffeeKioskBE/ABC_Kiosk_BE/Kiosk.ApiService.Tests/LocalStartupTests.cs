using AutomaticBrewingCoffee.API.Constants;
using Kiosk.ApiService.Extensions;
using Kiosk.ApiService.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Kiosk.ApiService.Tests;

public class LocalStartupTests
{
    [Fact]
    public async Task Health_endpoint_bypasses_api_key_but_ping_does_not()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConstantValue.ApiKeyName] = "local-kiosk-key"
            })
            .Build();
        var validator = new ApiKeyValidatorService(configuration);
        var middleware = new ApiKeyAuthenticationMiddleware(validator);

        var healthContext = new DefaultHttpContext();
        healthContext.Request.Path = "/health";
        var healthCalled = false;
        await middleware.InvokeAsync(healthContext, _ =>
        {
            healthCalled = true;
            return Task.CompletedTask;
        });

        var pingContext = new DefaultHttpContext();
        pingContext.Request.Path = "/api/v1/ping";
        var pingCalled = false;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => middleware.InvokeAsync(pingContext, _ =>
        {
            pingCalled = true;
            return Task.CompletedTask;
        }));

        Assert.True(healthCalled);
        Assert.False(pingCalled);
    }

    [Fact]
    public void Local_mode_disables_workflow_workers_by_default()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LOCAL_MODE"] = "true",
                ["WORKFLOW_WORKERS_ENABLED"] = "false"
            })
            .Build();

        Assert.False(KioskRuntimeSettings.AreWorkflowWorkersEnabled(configuration));
    }
}
