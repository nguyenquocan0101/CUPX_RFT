using Services.Dtos.DeviceModel;
using Services.Dtos.KioskVersion;

namespace Services.Dtos.KioskVersionDeviceModel;

public class KioskVersionDeviceModelInsideDto
{
    public string KioskVersionId { get; set; } = null!;

    public virtual KioskVersionInsideDto? KioskVersion { get; set; } = null!;

    public string DeviceModelId { get; set; } = null!;

    public virtual DeviceModelInsideDto? DeviceModel { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}