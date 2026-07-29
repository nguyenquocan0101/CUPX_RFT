using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Tests.DeviceService.TestData;
using Services.Tests.TestBase;

namespace Services.Tests.DeviceService;

[Collection("Test Collection")]
public class UpdateDeviceTest(StartUpTestBase fixture) : IAsyncLifetime
{
    private readonly AutoBrewingBeContext _dbContext = fixture.DbContext;
    private readonly Func<Task> _resetDatabase = fixture.ResetDatabaseAsync;
    private readonly IDeviceService _deviceService = fixture.ServiceProvider.GetRequiredService<IDeviceService>();
    private Device _deviceTest = new Device();

    public async Task InitializeAsync()
    {
        _deviceTest = await TestDataUpdateDevice.CreateDeviceTest(_dbContext);
    }

    public async Task DisposeAsync() => await _resetDatabase();

    [Fact]
    public async Task Handle_UpdateDeviceWithCorrectInput_ReturnsSuccessUpdated()
    {
        // Arrange
        var updateDeviceDto = TestDataUpdateDevice.CreateUpdateDeviceDto();

        var service = _deviceService.UpdateDevice(_deviceTest.DeviceId, updateDeviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = StatusCodes.Status202Accepted;
        var actual = result.StatusCode;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Handle_UpdateDeviceWithIncorrectInput_ReturnsNotFound()
    {
        // Arrange
        var updateDeviceDto = TestDataUpdateDevice.CreateUpdateDeviceDto();

        var service = _deviceService.UpdateDevice(_deviceTest.DeviceId + "1", updateDeviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = StatusCodes.Status404NotFound;
        var actual = result.StatusCode;

        Assert.Equal(expected, actual);
    }
}