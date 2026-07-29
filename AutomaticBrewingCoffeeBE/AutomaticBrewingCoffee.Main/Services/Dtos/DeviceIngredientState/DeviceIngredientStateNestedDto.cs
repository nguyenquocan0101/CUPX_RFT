using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.DeviceIngredientState;

public class DeviceIngredientStateNestedDto
{
    public double MaxCapacity { get; set; } = 0;

    public double MinCapacity { get; set; } = 0;

    public double WarningPercent { get; set; } = 0;

    [StringLength(100)] [Required] public string IngredientType { get; set; } = null!;

    public double CurrentCapacity { get; set; }

    [StringLength(10)] public string CapacityLevel { get; set; } = null!;

    [StringLength(20)] public string Unit { get; set; } = null!;

    public bool IsWarning { get; set; }

    // Is the device support regenerate this ingredient
    public bool IsRenewable { get; set; }

    // Is this ingredient is the primary ingredient support of the device
    public bool IsPrimary { get; set; }

    public DateTime? LastRefilledDate { get; set; }
}