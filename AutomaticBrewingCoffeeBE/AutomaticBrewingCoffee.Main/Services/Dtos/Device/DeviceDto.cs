using Services.Dtos.DeviceIngredientState;
using Services.Dtos.DeviceModel;

namespace Services.Dtos.Device;

public class DeviceDto
{
    public string DeviceId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? DeviceModelId { get; set; }

    public DeviceModelDto? DeviceModel { get; set; }

    public string SerialNumber { get; set; } = null!;

    public string Status { get; set; } = default!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? HubConnectionString { get; set; }

    public List<DeviceIngredientStateInsideDto>? DeviceIngredientStates { get; set; }
}