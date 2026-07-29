using Services.Dtos.DeviceModel;
using Services.Dtos.Workflow;

namespace Services.Dtos.Step;

public class StepDto
{
    public string StepId { get; set; } = null!;

    public string WorkflowId { get; set; } = null!;

    public string? Name { get; set; }

    public string Type { get; set; } = null!;

    public string DeviceModelId { get; set; } = null!;

    public DeviceModelDto? DeviceType { get; set; }

    public int Sequence { get; set; }

    public int? MaxRetries { get; set; }

    public string? CallbackWorkflowId { get; set; }

    public List<StepConditionDto>? Conditions { get; set; }

    public string? Parameters { get; set; }

    public virtual WorkflowDto Workflow { get; set; } = null!;

    public string? StepCode { get; set; }

    public string? CallbackStepCode { get; set; }
}