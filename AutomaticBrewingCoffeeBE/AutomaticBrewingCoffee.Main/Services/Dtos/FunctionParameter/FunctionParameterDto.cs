using Services.Dtos.DeviceFunction;

namespace Services.Dtos.FunctionParameter;

public class FunctionParameterDto
{
    public string FunctionParameterId { get; set; } = null!;

    public string DeviceFunctionId { get; set; } = null!;

    public virtual DeviceFunctionDto DeviceFunction { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string? Min { get; set; } = null!;

    public List<ParameterOptionDto>? Options { get; set; }

    public string? Max { get; set; } = null!;

    public string Default { get; set; } = null!;

    public string? Description { get; set; } = null!;
}