using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Tests.DeviceService.TestData;
using Services.Tests.TestBase;

namespace Services.Tests.DeviceService;

[Collection("Test Collection")]
public class GetDeviceTest(StartUpTestBase fixture) : IAsyncLifetime
{
    private readonly AutoBrewingBeContext _dbContext = fixture.DbContext;
    private readonly Func<Task> _resetDatabase = fixture.ResetDatabaseAsync;
    private readonly IDeviceService _deviceService = fixture.ServiceProvider.GetRequiredService<IDeviceService>();
    private Device _deviceTest = new Device();

    public async Task InitializeAsync()
    {
        _deviceTest = await TestDataGetDevice.CreateDeviceTest(_dbContext);
    }

    public async Task DisposeAsync() => await _resetDatabase();

    [Fact]
    public async Task Handle_GetDeviceWithCorrectRequest_ReturnsCorrectResponse()
    {
        // Arrange
        var deviceId = _deviceTest.DeviceId;
        var service = _deviceService.GetDevice(deviceId);

        // Act
        var result = await service;

        // Assert
        var expected = deviceId;
        var actual = result.Response!.DeviceId;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Handle_GetDeviceWithIncorrectRequest_ReturnsCorrectResponse()
    {
        // Arrange
        var deviceId = _deviceTest.DeviceId + "1";
        var service = _deviceService.GetDevice(deviceId);

        // Act
        var result = await service;

        // Assert
        // var expected = deviceId;
        // var actual = result.Response!.DeviceId;

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }
}