using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Workflow;

public class RemoveWorkflowDto
{
    [Required]
    public string WorkflowId { get; set; }
}