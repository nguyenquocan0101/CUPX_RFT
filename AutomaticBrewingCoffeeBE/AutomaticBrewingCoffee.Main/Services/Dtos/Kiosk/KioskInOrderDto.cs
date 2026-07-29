using Services.Dtos.Store;

namespace Services.Dtos.Kiosk;

public class KioskInOrderDto
{
    public string KioskId { get; set; } = null!;

    public string? KioskVersionId { get; set; }

    public string? MenuId { get; set; }

    public string? Position { get; set; } = null!;
    
    public string StoreId { get; set; } = null!;

    public virtual StoreInsideDto? Store { get; set; } = null!;

    public string Location { get; set; } = null!;
}