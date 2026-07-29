using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.SyncTask;

namespace Services.Dtos.SyncEvent;

public class SyncEventDto
{
    public string SyncEventId { get; set; } = null!;
    public string SyncEventType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string? SecondEntityId { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; } = null!;

    public virtual List<SyncTaskInSyncEventDto>? SyncTasks { get; set; }
}