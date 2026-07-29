using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Workflow : BaseModel
{
    [Key] [StringLength(50)] public string WorkflowId { get; set; } = null!;

    [StringLength(50)] public string? ProductId { get; set; }

    [StringLength(100)] public string? Name { get; set; }

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(50)] public string? KioskVersionId { get; set; }

    [Required] [StringLength(50)] public string Type { get; set; } = null!;

    [ForeignKey(nameof(ProductId))] public virtual Product? Product { get; set; }

    public virtual ICollection<Step>? Steps { get; set; } = new List<Step>();
}