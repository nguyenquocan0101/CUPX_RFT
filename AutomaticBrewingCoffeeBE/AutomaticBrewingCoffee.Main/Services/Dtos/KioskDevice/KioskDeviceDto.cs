using Services.Dtos.Device;
using Services.Dtos.Kiosk;

namespace Services.Dtos.KioskDevice;

public class KioskDeviceDto
{
    public string KioskDeviceMappingId { get; set; } = null!;

    public string? DeviceId { get; set; }
    public string? KioskId { get; set; }

    public string Status { get; set; } = null!;

    public string? Note { get; set; }

    public virtual DeviceDto? Device { get; set; }

    public virtual KioskDto? Kiosk { get; set; }
}