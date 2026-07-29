using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Workflow;

public class WorkflowDto
{
    public string WorkflowId { get; set; } = null!;

    public string? ProductId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
    public string Type { get; set; } = null!;


}