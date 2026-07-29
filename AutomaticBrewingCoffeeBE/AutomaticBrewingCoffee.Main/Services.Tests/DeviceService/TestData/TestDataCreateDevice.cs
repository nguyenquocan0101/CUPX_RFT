using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Device;

namespace Services.Tests.DeviceService.TestData;

public class TestDataCreateDevice
{
    public static DeviceModel CreateDeviceModel() => new DeviceModel()
    {
        DeviceModelId = "local-test-device-model",
        ModelName = "Local test device model",
        Status = EBaseStatus.Active.ToString(),
        DeviceIngredients = new List<DeviceIngredient>()
    };

    public static CreateDeviceDto CreateCreateDeviceDto() => new CreateDeviceDto()
    {
        DeviceModelId = "local-test-device-model",
        SerialNumber = "local-test-serial",
        Status = EDeviceStatus.Stock.ToString(),
        Name = "Stirrer",
        Description = "Stirs drinks",
    };

    public static CreateDeviceDto CreateCreateDeviceDto(EDeviceStatus status) => new CreateDeviceDto()
    {
        DeviceModelId = "local-test-device-model",
        SerialNumber = "local-test-serial",
        Status = status.ToString(),
        Name = "Stirrer",
        Description = "Stirs drinks",
    };

    public static CreateDeviceDto CreateCreateDeviceDto(string status) => new CreateDeviceDto()
    {
        DeviceModelId = "local-test-device-model",
        SerialNumber = "local-test-serial",
        Status = status,
        Name = "Stirrer",
        Description = "Stirs drinks",
    };

    public static UpdateDeviceDto CreateUpdateDeviceDto() => new UpdateDeviceDto()
    {
        SerialNumber = "local-test-serial",
        Status = EDeviceStatus.Stock.ToString(),
        Name = "Stirrer",
        Description = "Stirs drinks",
    };
}
