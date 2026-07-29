using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;

namespace Services.Dtos.DeviceModel;

public class DeviceModelQueryDto : BaseQuery
{
    public string? KioskVersionId { get; set; }
    public string? Status { get; set; }
}