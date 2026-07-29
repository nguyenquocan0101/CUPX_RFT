using Services.Base;

namespace Services.Dtos.SyncTask;

public class SyncTaskQueryDto : BaseQuery
{
    public string? SyncTaskId { get; set; } = null!;

    public string? SyncEventId { get; set; } = null!;

    public string? KioskId { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public bool? IsSynced { get; set; }

    public DateTime? SyncedAt { get; set; }
}