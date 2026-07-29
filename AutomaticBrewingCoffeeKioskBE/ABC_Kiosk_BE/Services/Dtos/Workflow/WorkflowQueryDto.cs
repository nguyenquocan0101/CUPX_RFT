using Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Workflow;

public class WorkflowQueryDto : BaseQuery
{
    public string? ProductId { get; set; }
    public WorkflowType? Type { get; set; }
}