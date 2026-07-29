using Domain.Enums;

namespace Services.Dtos.Sync;

public class WorkflowSyncDto
{
    public string WorkflowId { get; set; } = null!;

    public string? ProductId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public WorkflowType Type { get; set; }
}