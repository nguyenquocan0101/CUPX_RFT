using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.KioskDevice;

public class ChangeKioskDeviceStatusDto
{
    [Required]
    [MatchEnum(typeof(EKioskDeviceStatus))]
    public string Status { get; set; } = null!;
}