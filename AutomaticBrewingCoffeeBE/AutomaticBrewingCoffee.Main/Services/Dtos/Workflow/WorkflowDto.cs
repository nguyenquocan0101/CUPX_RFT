using Services.Dtos.Product;
using Services.Dtos.Step;

namespace Services.Dtos.Workflow;

public class WorkflowDto
{
    public string WorkflowId { get; set; } = null!;

    public string? ProductId { get; set; }

    public ProductDto? Product { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public string? KioskVersionId { get; set; }

    public int? TotalStep { get; set; }

    public virtual ICollection<StepInsideDto> Steps { get; set; } = new List<StepInsideDto>();
}