using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Device;

namespace Services.Tests.DeviceService.TestData;

public class TestDataCreateDevice
{
    public static CreateDeviceDto CreateCreateDeviceDto() => new CreateDeviceDto()
    {
        Status = EDeviceStatus.Stock.ToString(),
        Name = "Stirrer",
        Description = "Stirs drinks",
    };

    public static CreateDeviceDto CreateCreateDeviceDto(EDeviceStatus status) => new CreateDeviceDto()
    {
        Status = status.ToString(),
        Name = "Stirrer",
        Description = "Stirs drinks",
    };

    public static CreateDeviceDto CreateCreateDeviceDto(string status) => new CreateDeviceDto()
    {
        Status = status,
        Name = "Stirrer",
        Description = "Stirs drinks",
    };

    public static UpdateDeviceDto CreateUpdateDeviceDto() => new UpdateDeviceDto()
    {
        Status = EDeviceStatus.Stock.ToString(),
        Name = "Stirrer",
        Description = "Stirs drinks",
    };
}