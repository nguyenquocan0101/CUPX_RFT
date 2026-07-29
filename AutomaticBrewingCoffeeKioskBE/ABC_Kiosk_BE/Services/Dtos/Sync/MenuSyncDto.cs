using Domain.Enums;

namespace Services.Dtos.Sync;

public class MenuSyncDto
{
    public string MenuId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public BaseStatus Status { get; set; }
}