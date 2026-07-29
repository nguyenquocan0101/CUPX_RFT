using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public partial class Product
{
    [Key]
    [StringLength(50)]
    public string ProductId { get; set; } = null!;

    [StringLength(50)]
    public string? ParentId { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(300)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; }

    [StringLength(10)]
    public ProductSize? Size { get; set; }

    [StringLength(10)]
    public ProductType Type { get; set; }

    public DateTime? LastSync { get; set; }

    public virtual ICollection<IngredientProductMapping> IngredientProductMappings { get; set; } = new List<IngredientProductMapping>();

    public virtual ICollection<Product> InverseParent { get; set; } = new List<Product>();

    [ForeignKey("ParentId")]
    public virtual Product? Parent { get; set; }

}

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .Property(o => o.Size)
            .HasConversion(
                    v => v.ToString(),
                    v => (ProductSize)Enum.Parse(typeof(ProductSize), v)
                );
        builder
           .Property(o => o.Type)
           .HasConversion(
                   v => v.ToString(),
                   v => (ProductType)Enum.Parse(typeof(ProductType), v)
               );
    }
}