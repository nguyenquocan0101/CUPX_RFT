using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class DeviceIngredientHistory : BaseModel
{
    [Key] [StringLength(50)] [Required] public string DeviceIngredientHistoryId { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceIngredientStateId { get; set; } = null!;
    
    [StringLength(100)] [Required] public string IngredientType { get; set; } = null!;
    
    [StringLength(50)] [Required] public string DeviceId { get; set; } = null!;

    [ForeignKey(nameof(DeviceId))] public Device Device { get; set; } = null!;

    public double DeltaAmount { get; set; }

    public double OldCapacity { get; set; }

    public double NewCapacity { get; set; }

    public string? OrderId { get; set; }

    [ForeignKey(nameof(OrderId))] public Order? Order { get; set; }

    [StringLength(100)] public string? PerformedBy { get; set; }

    [StringLength(50)] public string Action { get; set; } = null!;
}