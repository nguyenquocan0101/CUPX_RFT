using Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public partial class Workflow
{
    [Key]
    [StringLength(50)]
    public string WorkflowId { get; set; } = null!;

    [StringLength(50)]
    public string? ProductId { get; set; } 

    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    [Required] 
    [StringLength(50)]
    public WorkflowType Type { get; set; }

   

    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }

    public virtual ICollection<Step> Steps { get; set; } = new List<Step>();
}

public class WorkflowConfig : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder
            .Property(o => o.Type)
            .HasConversion(
                    v => v.ToString(),
                    v => (WorkflowType)Enum.Parse(typeof(WorkflowType), v)
                );
    }
}