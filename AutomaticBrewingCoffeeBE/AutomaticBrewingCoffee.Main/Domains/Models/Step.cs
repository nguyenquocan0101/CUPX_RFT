using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Step : BaseModel
{
    [Key] [StringLength(50)] public string StepId { get; set; } = null!;

    [Required] [StringLength(50)] public string WorkflowId { get; set; } = null!;

    [StringLength(100)] public string? Name { get; set; }

    [Required] [StringLength(250)] public string Type { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;

    [StringLength(450)] public string? Conditions { get; set; }

    public int Sequence { get; set; }

    public int? MaxRetries { get; set; }

    [StringLength(50)] public string? StepCode { get; set; }

    [StringLength(50)] public string? CallbackStepCode { get; set; }

    [StringLength(50)] public string? CallbackWorkflowId { get; set; }

    [StringLength(500)] public string? Parameters { get; set; }

    [ForeignKey(nameof(WorkflowId))] public virtual Workflow Workflow { get; set; } = null!;
}