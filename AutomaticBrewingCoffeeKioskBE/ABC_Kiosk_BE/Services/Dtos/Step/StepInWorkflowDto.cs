namespace Services.Dtos.Step;

using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

public class StepInWorkflowDto
{
    [StringLength(100)]
    public string Name { get; set; }
    public StepType Type { get; set; }

    [Required]
    [StringLength(50)]
    public string DeviceId { get; set; } = null!;

    public int? MaxRetries { get; set; }

    [StringLength(50)]
    public string? CallbackWorkflowId { get; set; }

    [StringLength(500)]
    public string Parameters { get; set; }
}
