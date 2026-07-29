using AutomaticBrewingCoffee.Domain.Context;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Tests.TestBase;

namespace Services.Tests.FranchiseService;

public class RemoveFranchiseTest(StartUpTestBase fixture) : IAsyncLifetime
{
    private readonly AutoBrewingBeContext _dbContext = fixture.DbContext;
    private readonly Func<Task> _resetDatabase = fixture.ResetDatabaseAsync;
    private readonly IStoreService _deviceService = fixture.ServiceProvider.GetRequiredService<IStoreService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabase();
}