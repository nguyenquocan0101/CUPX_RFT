using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Device;

public class CreateDeviceDto
{
    [Required, StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Required, StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
    public string Description { get; set; }

    [Required, StringLength(50, ErrorMessage = "SerialNumber cannot exceed 50 characters.")]
    public string SerialNumber { get; set; }
    public DeviceStatus Status { get; set; } = default!;
}