using Services.Dtos.DeviceModel;
using Services.Dtos.FunctionParameter;

namespace Services.Dtos.DeviceFunction;

public class DeviceFunctionDto
{
    public string DeviceFunctionId { get; set; } = null!;

    public string DeviceModelId { get; set; } = null!;

    public virtual DeviceModelDto DeviceModel { get; set; } = null!;
    
    public string? Label { get; set; }
    
    public string Name { get; set; } = null!;

    public virtual IEnumerable<FunctionParameterDto>? FunctionParameters { get; set; }

    public string Status { get; set; } = null!;
}