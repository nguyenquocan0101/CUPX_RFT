using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Kiosk;

public class KioskDeviceOnPlaceQueryDto
{
    [StringLength(50)] public string? DeviceModelId { get; set; }

    [MatchEnum(typeof(EKioskDeviceOnPlaceStatus))]
    public string? WorkingStatus { get; set; }
}