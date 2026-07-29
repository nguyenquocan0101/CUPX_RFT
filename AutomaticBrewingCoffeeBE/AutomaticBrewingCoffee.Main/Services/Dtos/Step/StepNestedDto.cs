using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Step;

public class StepNestedDto
{
    [StringLength(100)] public string Name { get; set; } = null!;

    [Required] [StringLength(450)] public string Type { get; set; } = null!;

    [Required] public string DeviceModelId { get; set; } = null!;

    public int? MaxRetries { get; set; }

    public int Sequence { get; set; }

    [StringLength(50)] public string? CallbackStepId { get; set; }

    public List<StepConditionDto>? Conditions { get; set; }
    [StringLength(50)] public string? CallbackWorkflowId { get; set; }

    [StringLength(500)] public string? Parameters { get; set; }

    [StringLength(50)] public string? StepCode { get; set; }

    [StringLength(50)] public string? CallbackStepCode { get; set; }
}