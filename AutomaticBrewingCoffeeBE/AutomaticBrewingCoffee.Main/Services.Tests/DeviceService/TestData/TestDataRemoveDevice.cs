using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;

namespace Services.Tests.DeviceService.TestData;

public class TestDataRemoveDevice
{
    private static Device CreateDevice(EDeviceStatus status) => new Device()
    {
        DeviceId = Guid.NewGuid().ToString(),
        SerialNumber = Guid.NewGuid().ToString(),
        Name = "Cup Dropping Machine",
        Description = "Machine hold cup and drop",
        IsDeleted = false,
        DeletedDate = null,
        Status = status.ToString(),
        CreatedDate = DateTime.UtcNow,
        UpdatedDate = null,
    };

    public static async Task<Device> CreateDeviceTest(AutoBrewingBeContext dbContext)
    {
        var device = CreateDevice(EDeviceStatus.Stock);
        await dbContext.AddAsync(device);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return device;
    }
}
