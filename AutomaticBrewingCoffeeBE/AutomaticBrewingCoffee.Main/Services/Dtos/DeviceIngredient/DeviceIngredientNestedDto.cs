using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.DeviceIngredient;

public class DeviceIngredientNestedDto
{
    [StringLength(100)] [Required] public string Label { get; set; } = null!;

    [StringLength(100)]
    [Required]
    public string IngredientType { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    public double MaxCapacity { get; set; } = 0;

    public double MinCapacity { get; set; } = 0;

    public double WarningPercent { get; set; } = 0;

    [StringLength(20)]
    [MatchEnum(typeof(EBaseUnit))]
    public string Unit { get; set; } = null!;

    public bool IsRenewable { get; set; }

    public bool IsPrimary { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
    
    [StringLength(100)] [Required] public string? DeviceFunctionName { get; set; } = null!;
    
    [StringLength(100)]
    public string? IngredientSelectorParameter { get; set; }
    
    [StringLength(100)]
    public string? IngredientSelectorValue { get; set; }
    
    [StringLength(100)]
    public string? TargetOverrideParameter { get; set; } = null!;
}