namespace Services.Dtos.DeviceIngredient;

public class DeviceIngredientInsideDto
{
    public string DeviceIngredientId { get; set; } = null!;

    public string DeviceModelId { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string IngredientType { get; set; } = null!;

    public string? Description { get; set; }

    public double MaxCapacity { get; set; } = 0;

    public double MinCapacity { get; set; } = 0;

    public double WarningPercent { get; set; } = 0;

    public string Unit { get; set; } = null!;

    public bool IsRenewable { get; set; }

    public bool IsPrimary { get; set; }

    public string Status { get; set; } = null!;

    public string? DeviceFunctionName { get; set; } = null!;

    public string? IngredientSelectorParameter { get; set; }

    public string? IngredientSelectorValue { get; set; }

    public string? TargetOverrideParameter { get; set; } = null!;
}