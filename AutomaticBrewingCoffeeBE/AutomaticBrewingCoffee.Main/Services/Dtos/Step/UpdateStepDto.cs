using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;

namespace Services.Dtos.Step;

public class UpdateStepDto
{
    [StringLength(100)] public string Name { get; set; } = null!;

    [Required] [StringLength(450)] public string Type { get; set; } = null!;

    [Required] [StringLength(50)] public string WorkflowId { get; set; } = null!;

    [Required] public string DeviceModelId { get; set; } = null!;

    public int? MaxRetries { get; set; }

    [StringLength(50)] public string? CallbackStepId { get; set; }

    [StringLength(450)] public string? Conditions { get; set; }

    [StringLength(50)] public string? CallbackWorkflowId { get; set; } = null!;

    [StringLength(500)] public string? Parameters { get; set; } = null!;

    [StringLength(50)] public string? StepCode { get; set; }

    [StringLength(50)] public string? CallbackStepCode { get; set; }
}