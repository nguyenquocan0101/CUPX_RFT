namespace Services.Dtos.Sync;

public class MenuSyncDto
{
    public string MenuId { get; set; } = null!;

    public string? KioskId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;
}