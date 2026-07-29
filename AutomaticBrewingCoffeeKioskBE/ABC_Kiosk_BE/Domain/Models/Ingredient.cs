using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public partial class Ingredient
{
    [Key]
    [StringLength(50)]
    public string IngredientId { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(10)]
    public IngredientUnit Unit { get; set; }

    public DateTime? LastSync { get; set; }

    public virtual ICollection<IngredientProductMapping> IngredientProductMappings { get; set; } = new List<IngredientProductMapping>();
}
public class IngredientConfig : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder
         .Property(o => o.Unit)
         .HasConversion(
                 v => v.ToString(),
                 v => (IngredientUnit)Enum.Parse(typeof(IngredientUnit), v)
             );
    }
}