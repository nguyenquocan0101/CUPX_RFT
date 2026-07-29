using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Product : BaseModel
{
    [Key] [StringLength(50)] public string ProductId { get; set; } = null!;

    [StringLength(50)] public string? ParentId { get; set; }

    [StringLength(50)] public string? ProductCategoryId { get; set; }

    [ForeignKey(nameof(ParentId))] public virtual Product? Parent { get; set; }


    [ForeignKey(nameof(ProductCategoryId))]
    public virtual ProductCategory? ProductCategory { get; set; }

    [StringLength(100)] public string? TagName { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; } = null!;


    [StringLength(300)] public string? Description { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    [StringLength(10)] public string? Size { get; set; }

    [StringLength(10)] public string? Type { get; set; }

    [Precision(18, 2)] public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    public virtual ICollection<Product>? InverseParent { get; set; } = new List<Product>();

    public virtual ICollection<MenuProductMapping>? MenuProductMappings { get; set; } = new List<MenuProductMapping>();

    public virtual ICollection<Workflow>? Workflows { get; set; } = new List<Workflow>();

    public IEnumerable<ProductAttribute>? ProductAttributes { get; set; } = new List<ProductAttribute>();
}