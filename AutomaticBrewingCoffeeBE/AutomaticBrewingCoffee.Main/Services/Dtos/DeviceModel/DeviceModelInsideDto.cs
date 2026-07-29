using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.DeviceType;

namespace Services.Dtos.DeviceModel;

public class DeviceModelInsideDto
{
    public string DeviceModelId { get; set; } = null!;

    public string? ModelName { get; set; } = null!;

    public string? Manufacturer { get; set; } = null!;

    public string? DeviceTypeId { get; set; }

    public virtual DeviceTypeDto? DeviceType { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; } = null!;
    
}