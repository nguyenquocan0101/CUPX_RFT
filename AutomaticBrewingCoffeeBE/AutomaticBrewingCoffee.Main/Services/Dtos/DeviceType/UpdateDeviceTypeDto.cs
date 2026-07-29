using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.DeviceType;

public class UpdateDeviceTypeDto
{
    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    [StringLength(10)]
    [Required]
    [MatchEnum(typeof(EBaseStatus))]
    public string Status { get; set; } = null!;

    public bool IsMobileDevice { get; set; }
}