using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using Services.Tests.DeviceService.TestData;
using Services.Tests.TestBase;

namespace Services.Tests.DeviceService;

[Collection("Test Collection")]
public class GetDevicesTest(StartUpTestBase fixture) : IAsyncLifetime
{
    private readonly AutoBrewingBeContext _dbContext = fixture.DbContext;
    private readonly Func<Task> _resetDatabase = fixture.ResetDatabaseAsync;
    private readonly IDeviceService _deviceService = fixture.ServiceProvider.GetRequiredService<IDeviceService>();
    private List<Device> _devicesTest = new List<Device>();

    public async Task InitializeAsync()
    {
        _devicesTest = await TestDataGetDevices.CreateDevicesTest(_dbContext);
    }

    public async Task DisposeAsync() => await _resetDatabase();

    [Fact]
    public async Task Handle_GetDevicesWithEmptyQuery_ReturnsCorrectTotal()
    {
        // Arrange
        var createDeviceDto = TestDataGetDevices.CreateDeviceQueryDto();

        var service = _deviceService.GetDevices(createDeviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = 10;
        var actual = result.Response?.Items.Count;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Handle_GetDevicesWithBlankQuery_ReturnsCorrectTotal()
    {
        // Arrange
        var createDeviceDto = TestDataGetDevices.CreateDeviceQueryDto(
            "",
            "",
            "",
            ""
        );

        var service = _deviceService.GetDevices(createDeviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = 0;
        var actual = result.Response?.Items.Count;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Handle_GetDevicesWithIdleStatus_ReturnsCorrectTotal()
    {
        // Arrange
        var createDeviceDto = TestDataGetDevices.CreateDeviceQueryDto(
            EDeviceStatus.Stock,
            null,
            null,
            null
        );
        var service = _deviceService.GetDevices(createDeviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = _devicesTest.Count(x => x.Status == EDeviceStatus.Stock.ToString());
        var actual = result.Response!.Total;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Handle_GetDevicesWithWorkingStatus_ReturnsCorrectTotal()
    {
        // Arrange
        var createDeviceDto = TestDataGetDevices.CreateDeviceQueryDto(
            EDeviceStatus.Working,
            null,
            null,
            null
        );
        var service = _deviceService.GetDevices(createDeviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = _devicesTest.Count(x => x.Status == EDeviceStatus.Working.ToString());
        var actual = result.Response!.Total;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Handle_GetDevicesWithBrokenStatus_ReturnsCorrectTotal()
    {
        // Arrange
        var createDeviceDto = TestDataGetDevices.CreateDeviceQueryDto(
            EDeviceStatus.Maintain,
            null,
            null,
            null
        );
        var service = _deviceService.GetDevices(createDeviceDto);

        // Act
        var result = await service;

        // Assert
        var expected = _devicesTest.Count(x => x.Status == EDeviceStatus.Maintain.ToString());
        var actual = result.Response!.Total;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Handle_GetDevicesWithPage1Size12_ReturnsCorrectTotal()
    {
        // Arrange
        var createDeviceDto = TestDataGetDevices.CreateDeviceQueryDto(
            null,
            null,
            null,
            null,
            true,
            1,
            12
        );
        var service = _deviceService.GetDevices(createDeviceDto);

        // Act
        var result = await service;

        // Assert

        var pageExpected = 1;
        var pageActual = result.Response!.Page;
        var sizeExpected = 12;
        var sizeActual = result.Response!.Items.Count;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(pageExpected, pageActual);
        Assert.Equal(sizeExpected, sizeActual);
    }

    [Fact]
    public async Task Handle_GetDevicesWithPage3Size5_ReturnsCorrectTotal()
    {
        // Arrange
        var createDeviceDto = TestDataGetDevices.CreateDeviceQueryDto(
            null,
            null,
            null,
            null,
            true,
            3,
            5
        );
        var service = _deviceService.GetDevices(createDeviceDto);

        // Act
        var result = await service;

        // Assert

        var pageExpected = 3;
        var pageActual = result.Response!.Page;
        var sizeExpected = 5;
        var sizeActual = result.Response!.Items.Count;

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(pageExpected, pageActual);
        Assert.Equal(sizeExpected, sizeActual);
    }
}