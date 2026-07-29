using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Services.Dtos.Device;
using Services.Dtos.DeviceIngredient;

namespace Services.Dtos.DeviceIngredientState;

public class DeviceIngredientStateDto
{
    public string DeviceIngredientStateId { get; set; } = null!;

    public string DeviceId { get; set; } = null!;

    public DeviceDto? Device { get; set; }

    public double MaxCapacity { get; set; } = 0;

    public double MinCapacity { get; set; } = 0;

    public double WarningPercent { get; set; } = 0;

    public string IngredientType { get; set; } = null!;

    public double CurrentCapacity { get; set; }

    public string CapacityLevel { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public bool IsWarning { get; set; }

    // Is the device support regenerate this ingredient
    public bool IsRenewable { get; set; }

    // Is this ingredient is the primary ingredient support of the device
    public bool IsPrimary { get; set; }

    public DateTime? LastRefilledDate { get; set; }
}