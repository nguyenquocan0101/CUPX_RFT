using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class SyncTask : BaseModel
{
    [Key] [StringLength(50)] public string SyncTaskId { get; set; } = null!;

    [StringLength(50)] public string SyncEventId { get; set; } = null!;

    [StringLength(50)] public string KioskId { get; set; } = null!;

    [ForeignKey(nameof(KioskId))] public Kiosk? Kiosk { get; set; }

    [ForeignKey(nameof(SyncEventId))] public SyncEvent? SyncEvent { get; set; } = null!;

    public bool IsSynced { get; set; }
    public DateTime? SyncedAt { get; set; }

    public void Sync()
    {
        IsSynced = true;
        SyncedAt = DateTime.UtcNow;
    }

    public void Async()
    {
        IsSynced = false;
        SyncedAt = null;
    }
}