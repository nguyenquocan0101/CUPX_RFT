using Services.Dtos.Device;

namespace Services.Dtos.KioskDevice;

public class KioskDeviceInsideDto
{
    public string KioskDeviceMappingId { get; set; } = null!;

    public string? DeviceId { get; set; }
    
    public string? KioskId { get; set; }

    public string Status { get; set; } = null!;
    
    public string? Side { get; set; }
    
    public bool IsDisposed { get; set; } = false;

    public DateTime? DisposedDate { get; set; }

    public string? Note { get; set; }

    public virtual DeviceInsideDto? Device { get; set; }
    
}