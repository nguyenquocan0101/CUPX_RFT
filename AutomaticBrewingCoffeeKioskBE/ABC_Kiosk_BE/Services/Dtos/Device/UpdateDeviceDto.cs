using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Device;

public class UpdateDeviceDto
{
    // DB: varchar(100), Nullable
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = null!;

    // DB: varchar(300), Nullable
    [Required]
    [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
    public string Description { get; set; } = null!;
    public DeviceStatus Status { get; set; } = default!;
}