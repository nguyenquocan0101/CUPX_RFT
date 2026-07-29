using Services.Dtos.DeviceIngredientHistory;
using Services.Dtos.DeviceIngredientState;
using Services.Dtos.DeviceModel;

namespace Services.Dtos.Device;

public class DeviceInsideDto
{
    public string DeviceId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? DeviceModelId { get; set; }

    public DeviceModelInsideDto? DeviceModel { get; set; }

    public string SerialNumber { get; set; } = null!;

    public string Status { get; set; } = default!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public List<DeviceIngredientStateInsideDto>? DeviceIngredientStates { get; set; }

    public List<DeviceIngredientHistoryInsideDto>? DeviceIngredientHistories { get; set; }
}