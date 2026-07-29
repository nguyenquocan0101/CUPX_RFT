using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Step;

public class ReorderStepDto
{
    [Required] public string DragStepId { get; set; } = null!;
    [Required] public string TargetStepId { get; set; } = null!;
    [Required] public bool InsertAfter { get; set; }
}