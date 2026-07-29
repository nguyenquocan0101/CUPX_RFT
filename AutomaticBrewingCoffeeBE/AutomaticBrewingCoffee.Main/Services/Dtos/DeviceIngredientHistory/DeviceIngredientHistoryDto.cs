using Services.Dtos.Device;

namespace Services.Dtos.DeviceIngredientHistory;

public class DeviceIngredientHistoryDto
{
    public string DeviceIngredientHistoryId { get; set; } = null!;

    public string DeviceIngredientStateId { get; set; } = null!;
    
    public string IngredientType { get; set; } = null!;

    public string DeviceId { get; set; } = null!;

    public DeviceDto? Device { get; set; }

    public double DeltaAmount { get; set; }

    public double OldCapacity { get; set; }

    public double NewCapacity { get; set; }

    public string? PerformedBy { get; set; }

    public string Action { get; set; } = null!;
}