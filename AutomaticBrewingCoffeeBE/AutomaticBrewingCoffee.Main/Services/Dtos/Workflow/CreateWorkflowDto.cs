using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.Step;
using Services.Validations;

namespace Services.Dtos.Workflow;

public class CreateWorkflowDto
{
    [StringLength(50)] public string? ProductId { get; set; }

    [StringLength(100)] public string? Name { get; set; }

    [StringLength(300)] public string? Description { get; set; }
    
    [StringLength(50)] public string? KioskVersionId { get; set; }

    [Required]
    [MatchEnum(typeof(EWorkflowType))]
    public string Type { get; set; } = null!;

    public virtual ICollection<StepNestedDto> Steps { get; set; } = new List<StepNestedDto>();
}