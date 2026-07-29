namespace Services.Dtos.KioskDevice;

public class KioskDeviceOnHubDto
{
    public string? Status { get; set; }

    public DateTime? StatusUpdatedTime { get; set; }

    public string? ConnectionState { get; set; }

    public DateTime? ConnectionStateUpdatedTime { get; set; }

    public DateTime? LastActivityTime { get; set; }

    public int CloudToDeviceMessageCount { get; set; }

    public string? ConnectionString { get; set; }
}