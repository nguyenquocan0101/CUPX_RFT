using Services.Dtos.DeviceModel;
using Services.Dtos.FunctionParameter;

namespace Services.Dtos.DeviceFunction;

public class DeviceFunctionInsideDto
{
    public string DeviceFunctionId { get; set; } = null!;

    public string DeviceModelId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Label { get; set; }

    public virtual IEnumerable<FunctionParameterInsideDto>? FunctionParameters { get; set; }

    public string Status { get; set; } = null!;
}