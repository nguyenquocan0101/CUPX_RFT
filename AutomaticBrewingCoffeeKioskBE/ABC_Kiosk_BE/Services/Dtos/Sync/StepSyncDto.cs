using Domain.Enums;

namespace Services.Dtos.Sync;

public class StepSyncDto
{
    public string StepId { get; set; } = null!;

    public string WorkflowId { get; set; } = null!;

    public string? Name { get; set; }
    public string? Type { get; set; }
    public string DeviceModelId { get; set; }
    public int Sequence { get; set; }

    public int? MaxRetries { get; set; }

    public string? CallbackWorkflowId { get; set; }

    public string? Parameters { get; set; }
    public string? CallbackStepCode { get; set; }
    public string? StepCode { get; set; }
    public List<StepConditionRawSyncDto>? Conditions { get; set; }
}