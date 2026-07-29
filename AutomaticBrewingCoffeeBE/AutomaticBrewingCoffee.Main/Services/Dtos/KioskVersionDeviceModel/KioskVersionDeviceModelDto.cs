using Services.Dtos.DeviceModel;
using Services.Dtos.KioskVersion;

namespace Services.Dtos.KioskVersionDeviceModel;

public class KioskVersionDeviceModelDto
{
    public string KioskVersionId { get; set; } = null!;

    public virtual KioskVersionDto? KioskVersion { get; set; } = null!;

    public string DeviceModelId { get; set; } = null!;

    public virtual DeviceModelDto? DeviceModel { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}