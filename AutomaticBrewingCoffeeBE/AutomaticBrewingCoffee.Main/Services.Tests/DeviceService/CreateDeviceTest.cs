using AutomaticBrewingCoffee.Domain.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Tests.DeviceService.TestData;
using Services.Tests.TestBase;

namespace Services.Tests.DeviceService;

[Collection("Test Collection")]
public class CreateDeviceTest(StartUpTestBase fixture) : IAsyncLifetime
{
    private readonly AutoBrewingBeContext _dbContext = fixture.DbContext;
    private readonly Func<Task> _resetDatabase = fixture.ResetDatabaseAsync;
    private readonly IDeviceService _deviceService = fixture.ServiceProvider.GetRequiredService<IDeviceService>();

    public async Task InitializeAsync()
    {
        await _dbContext.AddAsync(TestDataCreateDevice.CreateDeviceModel());
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }

    public async Task DisposeAsync() => await _resetDatabase();

    [Fact]
    public async Task Handle_CreateDeviceWithCorrectInput_ReturnsSuccessCreated()
    {
        // Arrange
        var deviceDto = TestDataCreateDevice.CreateCreateDeviceDto();

        var service = _deviceService.CreateDevice(deviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = StatusCodes.Status201Created;
        var actual = result.StatusCode;

        Assert.Equal(expected, actual);
    }
}
