namespace Services.Dtos.Sync;

public class DeviceSyncDto
{
    public string DeviceId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? DeviceModelId { get; set; }

    public string SerialNumber { get; set; } = null!;

    public string Status { get; set; } = default!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}