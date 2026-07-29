using Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Models;

public partial class Step
{
    [Key]
    [StringLength(50)]
    public string StepId { get; set; } = null!;

    [StringLength(50)]
    public string? WorkflowId { get; set; } 

    [StringLength(100)]
    public string? Name { get; set; }
    [Required]
    public string Function { get; set; }
    [Required]
    public string DeviceModelId { get; set; }
    [Required] 
    public int Sequence { get; set; }

    public int? MaxRetries { get; set; }

    [StringLength(50)]
    public string? CallbackWorkflowId { get; set; }
    public string? CallbackStepCode { get; set; }
    public string? StepCode { get; set; }

    [StringLength(500)]
    public string? Parameters { get; set; }
    [ForeignKey("WorkflowId")]
    [JsonIgnore]
    public virtual Workflow? Workflow { get; set; }
    public List<StepConditionRaw>? Conditions { get; set; }
}
