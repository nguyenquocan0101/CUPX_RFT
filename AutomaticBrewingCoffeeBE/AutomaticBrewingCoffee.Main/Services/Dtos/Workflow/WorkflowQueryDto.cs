using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Workflow;

public class WorkflowQueryDto : BaseQuery
{
    public string? ProductId { get; set; }
    [MatchEnum(typeof(EWorkflowType))] public string? WorkflowType { get; set; }
}