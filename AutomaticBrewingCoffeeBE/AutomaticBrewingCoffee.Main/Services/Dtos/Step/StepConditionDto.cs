using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Step;

public class StepConditionDto
{
    [StringLength(50)]
    [Required]
    [MatchEnum(typeof(EConditionName))]
    public string Name { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    public StepExpressionDto? Expression { get; set; } = null!;
}