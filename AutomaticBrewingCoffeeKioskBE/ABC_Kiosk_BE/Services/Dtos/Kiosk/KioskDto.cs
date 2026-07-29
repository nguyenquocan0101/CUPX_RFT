using Services.Dtos.Device;

namespace Services.Dtos.Kiosk;

public class KioskDto
{
    public string KioskId { get; set; } = null!;

    public string FranchiseId { get; set; } = null!;

    public IEnumerable<DeviceDto>? Devices { get; set; }

    public string? Location { get; set; }

    public string Status { get; set; } = default!;

    public DateTime InstalledDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}