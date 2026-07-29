namespace Services.Dtos.Sync;

public class OverridenKioskDataDto
{
    public List<StepSyncDto>? Steps { get; set; } = null!;
    public List<WorkflowSyncDto>? Workflows { get; set; } = null!;
    public List<DeviceSyncDto>? Devices { get; set; } = null!;
}