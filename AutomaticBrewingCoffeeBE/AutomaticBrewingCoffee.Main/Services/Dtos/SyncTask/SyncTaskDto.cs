using Services.Dtos.Kiosk;
using Services.Dtos.SyncEvent;

namespace Services.Dtos.SyncTask;

public class SyncTaskDto
{
    public string SyncTaskId { get; set; } = null!;

    public string SyncEventId { get; set; } = null!;

    public string KioskId { get; set; } = null!;
    
    public KioskInSyncTaskDto? Kiosk { get; set; }

    public SyncEventDto? SyncEvent { get; set; }

    public bool IsSynced { get; set; }

    public DateTime? SyncedAt { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; } = null!;

    public DateTime? DeletedDate { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
}