using Services.Dtos.Store;

namespace Services.Dtos.Kiosk;

public class KioskInSyncTaskDto
{
    public string KioskId { get; set; } = null!;

    public string? KioskVersionId { get; set; }

    public string? MenuId { get; set; }

    public string? Position { get; set; } = null!;

    public string StoreId { get; set; } = null!;

    public StoreInsideDto? Store { get; set; }

    public string Location { get; set; } = null!;
}