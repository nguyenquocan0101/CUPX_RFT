using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Device;

namespace Services.Tests.DeviceService.TestData;

public class TestDataGetDevices
{
    private static List<Device> CreateDevices() => new List<Device>
    {
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Cup Dropping Machine",
            Description = "Drops cups automatically", IsDeleted = false, DeletedDate = null,
            Status = EDeviceStatus.Stock.ToString(), CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Coffee Brewer A", Description = "Brews coffee - Model A",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Maintain.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Milk Frother", Description = "Froths milk for lattes",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Sugar Dispenser", Description = "Dispenses sugar",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Robot Arm 1", Description = "Picks and places cups",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Robot Arm 2", Description = "Assists in assembly",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Maintain.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Tablet Controller", Description = "UI for placing orders",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Heater", Description = "Keeps drinks warm", IsDeleted = false,
            DeletedDate = null, Status = EDeviceStatus.Maintain.ToString(), CreatedDate = DateTime.UtcNow,
            UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Cooler", Description = "Cools drinks", IsDeleted = false,
            DeletedDate = null, Status = EDeviceStatus.Stock.ToString(), CreatedDate = DateTime.UtcNow,
            UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Payment Terminal", Description = "Handles payment",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Water Pump", Description = "Pumps water", IsDeleted = false,
            DeletedDate = null, Status = EDeviceStatus.Working.ToString(), CreatedDate = DateTime.UtcNow,
            UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Cup Sensor", Description = "Detects cup presence",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Bean Grinder", Description = "Grinds coffee beans",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Trash Compactor", Description = "Disposes waste",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Stock.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Stirrer", Description = "Stirs drinks", IsDeleted = false,
            DeletedDate = null, Status = EDeviceStatus.Working.ToString(), CreatedDate = DateTime.UtcNow,
            UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Mixer", Description = "Mixes ingredients", IsDeleted = false,
            DeletedDate = null, Status = EDeviceStatus.Stock.ToString(), CreatedDate = DateTime.UtcNow,
            UpdatedDate = null
        },
        new Device
        {
            DeviceId = Guid.NewGuid().ToString(), Name = "Espresso Maker", Description = "Makes espresso shots",
            IsDeleted = false, DeletedDate = null, Status = EDeviceStatus.Working.ToString(),
            CreatedDate = DateTime.UtcNow, UpdatedDate = null
        }
    };

    public static async Task<List<Device>> CreateDevicesTest(AutoBrewingBeContext dbContext)
    {
        var devices = CreateDevices();
        await dbContext.AddRangeAsync(devices);
        await dbContext.SaveChangesAsync();
        return devices;
    }

    public static DeviceQueryDto CreateDeviceQueryDto() => new DeviceQueryDto() { };

    public static DeviceQueryDto CreateDeviceQueryDto(
        string? status,
        string? filterBy,
        string? filterQuery,
        string? sortBy,
        bool isAsc = true,
        int page = 1,
        int size = 10
    ) => new DeviceQueryDto()
    {
        Status = status,
        FilterBy = filterBy,
        FilterQuery = filterQuery,
        SortBy = sortBy,
        IsAsc = isAsc,
        Page = page,
        Size = size
    };

    public static DeviceQueryDto CreateDeviceQueryDto(
        EDeviceStatus status,
        string? filterBy,
        string? filterQuery,
        string? sortBy,
        bool isAsc = true,
        int page = 1,
        int size = 10
    ) => new DeviceQueryDto()
    {
        Status = status.ToString(),
        FilterBy = filterBy,
        FilterQuery = filterQuery,
        SortBy = sortBy,
        IsAsc = isAsc,
        Page = page,
        Size = size
    };
}