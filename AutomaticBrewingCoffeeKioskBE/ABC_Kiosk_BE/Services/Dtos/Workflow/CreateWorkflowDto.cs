using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Services.Dtos.Step;
using Services.Validations;

namespace Services.Dtos.Workflow;

public class CreateWorkflowDto
{

    [StringLength(50)] public string? ProductId { get; set; }

    [StringLength(100)] public string? Name { get; set; }

    [StringLength(300)] public string? Description { get; set; }
    public WorkflowType Type { get; set; }

    public virtual ICollection<StepInWorkflowDto> Steps { get; set; } = new List<StepInWorkflowDto>();
}