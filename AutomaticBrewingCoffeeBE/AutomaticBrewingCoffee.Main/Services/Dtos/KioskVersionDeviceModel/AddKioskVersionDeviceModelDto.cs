using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.KioskVersionDeviceModel;

public class AddKioskVersionDeviceModelDto
{
    [StringLength(50)] [Required] public string KioskVersionId { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}