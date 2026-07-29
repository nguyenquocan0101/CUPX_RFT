using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Device;

public class CreateDeviceDto
{
    [StringLength(50)] public string? DeviceModelId { get; set; }

    [StringLength(100)] public string SerialNumber { get; set; } = null!;

    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(300)] [Required] public string Description { get; set; } = null!;

    [Required]
    [MatchEnum(typeof(EDeviceStatus))]
    public string Status { get; set; } = default!;
}