using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;

namespace AutomaticBrewingCoffee.Domain.Models;

public class SyncEvent : BaseModel
{
    [Key] [StringLength(50)] public string SyncEventId { get; set; } = null!;
    [StringLength(50)] public string SyncEventType { get; set; } = null!;
    [StringLength(50)] public string EntityType { get; set; } = null!;
    [StringLength(50)] public string EntityId { get; set; } = null!;
    [StringLength(50)] public string? SecondEntityId { get; set; } = null!;

    public virtual List<SyncTask>? SyncTasks { get; set; }
}