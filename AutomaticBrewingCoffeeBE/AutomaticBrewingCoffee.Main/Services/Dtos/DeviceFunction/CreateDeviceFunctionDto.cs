using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.FunctionParameter;
using Services.Validations;

namespace Services.Dtos.DeviceFunction;

public class CreateDeviceFunctionDto
{
    [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;

    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    public IEnumerable<FunctionParameterNestedDto>? FunctionParameters { get; set; }
    
    [StringLength(100)] public string? Label { get; set; }

    [StringLength(10)]
    [MatchEnum(typeof(EBaseStatus))]
    [Required]
    public string Status { get; set; } = null!;
}