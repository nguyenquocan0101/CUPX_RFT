using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.DeviceIngredientState;

public class UpdateDeviceIngredientStateDto
{
    public double WarningPercent { get; set; }

    public double CurrentCapacity { get; set; }

    public bool IsWarning { get; set; }

    // Is the device support regenerate this ingredient
    public bool IsRenewable { get; set; }

    // Is this ingredient is the primary ingredient support of the device
    public bool IsPrimary { get; set; }
}