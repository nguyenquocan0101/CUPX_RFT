using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.KioskDevice;

public class AddKioskDeviceDto
{
    [Required] public string KioskId { get; set; } = null!;
    [Required] public string DeviceId { get; set; } = null!;
}