using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.SyncEvent;

public class SyncEventQueryDto : BaseQuery
{
    public string? SyncEventId { get; set; } = null!;

    [MatchEnum(typeof(ESyncEventType))] public string? SyncEventType { get; set; } = null!;

    [MatchEnum(typeof(ESyncEntityType))] public string? EntityType { get; set; } = null!;

    public string? EntityId { get; set; } = null!;

    public string? SecondEntityId { get; set; } = null!;
    
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; } = null!;
    public DateTime? DeletedDate { get; set; } = null!;
    public bool? IsDeleted { get; set; } = false;
}