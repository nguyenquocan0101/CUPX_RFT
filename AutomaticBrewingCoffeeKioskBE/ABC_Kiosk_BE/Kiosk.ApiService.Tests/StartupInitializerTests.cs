using Kiosk.ApiService.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Services.Interfaces;

namespace Kiosk.ApiService.Tests;

public class StartupInitializerTests
{
    [Fact]
    public async Task Initialize_ensures_resources_without_destructive_operations()
    {
        var provisioner = new RecordingStartupResourceProvisioner();
        var runtimeState = new RecordingRuntimeStateService { Maintenance = true };
        var initializer = new StartupInitializer(
            NullLogger<StartupInitializer>.Instance,
            runtimeState,
            provisioner);

        await initializer.InitializeAsync();

        Assert.Equal(1, provisioner.EnsureCouchDatabaseCalls);
        Assert.Equal(1, provisioner.EnsureRabbitTopologyCalls);
        Assert.True(runtimeState.SetMaintenanceCalled);
        Assert.False(runtimeState.Maintenance);
        Assert.Equal(0, provisioner.DeleteCalls);
        Assert.Equal(
            ["devicestatusdocuments", "devicedocuments", "workflowdatas"],
            StartupResourceProvisioner.RequiredCouchDatabases);
    }

    private sealed class RecordingStartupResourceProvisioner : IStartupResourceProvisioner
    {
        public int EnsureCouchDatabaseCalls { get; private set; }
        public int EnsureRabbitTopologyCalls { get; private set; }
        public int DeleteCalls { get; private set; }

        public Task EnsureCouchDatabasesAsync(CancellationToken cancellationToken = default)
        {
            EnsureCouchDatabaseCalls++;
            return Task.CompletedTask;
        }

        public Task EnsureRabbitMqTopologyAsync(CancellationToken cancellationToken = default)
        {
            EnsureRabbitTopologyCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingRuntimeStateService : IRuntimeStateService
    {
        public bool Maintenance { get; set; }
        public bool SetMaintenanceCalled { get; private set; }

        public Task<bool> IsMaintenanceAsync() => Task.FromResult(Maintenance);

        public Task SetMaintenanceAsync(bool on)
        {
            Maintenance = on;
            SetMaintenanceCalled = true;
            return Task.CompletedTask;
        }
    }
}
