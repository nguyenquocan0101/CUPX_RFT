using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Step;

public class CreateStepDto
{
    [StringLength(100)]
    public string Name { get; set; }
    public StepType Type { get; set; }
    public DeviceType DeviceType { get; set; }

    public int? MaxRetries { get; set; }

    [StringLength(50)]
    public string CallbackWorkflowId { get; set; }

    [StringLength(500)]
    public string Parameters { get; set; }
}
