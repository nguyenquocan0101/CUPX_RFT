using System.ComponentModel.DataAnnotations;
using Services.Dtos.FunctionParameter;

namespace Services.Dtos.DeviceFunction;

public class UpdateDeviceFunctionDto
{
    [StringLength(50)] [Required] public string DeviceFunctionId { get; set; } = null!;
    
    [StringLength(100)] public string? Label { get; set; }
    
    [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;
    
    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    public virtual IEnumerable<FunctionParameterNestedDto>? FunctionParameters { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}