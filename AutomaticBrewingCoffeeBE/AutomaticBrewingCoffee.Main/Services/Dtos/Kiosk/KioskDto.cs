using Services.Dtos.Device;
using Services.Dtos.KioskDevice;
using Services.Dtos.KioskVersion;
using Services.Dtos.Store;
using Services.Dtos.Webhook;

namespace Services.Dtos.Kiosk;

public class KioskDto
{
    public string KioskId { get; set; } = null!;

    public string? KioskVersionId { get; set; }

    public virtual KioskVersionInsideDto? KioskVersion { get; set; }

    public string? MenuId { get; set; }

    public string? ApiKey { get; set; }

    public string? Hostname { get; set; }

    public string? Position { get; set; } = null!;

    public string? TunnelToken { get; set; }

    public DateTime? WarrantyTime { get; set; }

    public string StoreId { get; set; } = null!;

    public virtual StoreInsideDto? Store { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime InstalledDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }


    public List<WebhookDto>? Webhooks { get; set; }

    public virtual IEnumerable<KioskDeviceInsideDto> KioskDevices { get; set; } = new List<KioskDeviceInsideDto>();


    // Props just exist only in dto

    public bool IsOnline { get; set; }
    
    public bool IsBusy { get; set; }
}