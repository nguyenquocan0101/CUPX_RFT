using System.ComponentModel.DataAnnotations;
using Services.Dtos.FunctionParameter;

namespace Services.Dtos.DeviceFunction;

public class DeviceFunctionNestedDto
{
    [StringLength(100)] [Required] public string Name { get; set; } = null!;
    
    [StringLength(100)] public string? Label { get; set; }

    public IEnumerable<FunctionParameterNestedDto>? FunctionParameters { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}