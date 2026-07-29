using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.FunctionParameter;

public class FunctionParameterNestedDto
{
    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(450)] public string? Min { get; set; } = null!;

    public List<ParameterOptionDto>? Options { get; set; }

    [StringLength(450)] public string? Max { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; } = null!;

    [StringLength(50)]
    [MatchEnum(typeof(EParameterType))]
    public string Type { get; set; } = null!;

    [StringLength(10)] [Required] public string Default { get; set; } = null!;
}