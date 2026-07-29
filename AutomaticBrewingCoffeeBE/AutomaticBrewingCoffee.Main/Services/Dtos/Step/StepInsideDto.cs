using Services.Dtos.DeviceModel;

namespace Services.Dtos.Step;

public class StepInsideDto
{
    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string DeviceModelId { get; set; } = null!;

    public DeviceModelDto? DeviceModel { get; set; }

    public int? MaxRetries { get; set; }

    public int Sequence { get; set; }

    public string? CallbackStepId { get; set; }

    public List<StepConditionDto>? Conditions { get; set; }

    public string? CallbackWorkflowId { get; set; }

    public string? Parameters { get; set; }

    public string? StepCode { get; set; }

    public string? CallbackStepCode { get; set; }
}