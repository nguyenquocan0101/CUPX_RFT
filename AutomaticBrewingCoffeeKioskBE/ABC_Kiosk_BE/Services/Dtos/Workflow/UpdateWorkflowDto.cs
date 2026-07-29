using Domain.Enums;
using Services.Dtos.Step;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Workflow;

public class UpdateWorkflowDto
{
    [Required]
    public string Id { get; set; } = null!;
    [StringLength(50)] public string? ProductId { get; set; }

    [StringLength(100)] public string? Name { get; set; }

    [StringLength(300)] public string? Description { get; set; }
    public WorkflowType Type { get; set; }

    public virtual ICollection<CreateStepDto> Steps { get; set; } = new List<CreateStepDto>();
}