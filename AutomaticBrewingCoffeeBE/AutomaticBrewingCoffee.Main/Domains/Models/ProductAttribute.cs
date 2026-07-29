using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class ProductAttribute : BaseModel
{
    [Key] [StringLength(50)] public string ProductAttributeId { get; set; } = null!;

    [StringLength(50)] public string ProductId { get; set; } = null!;

    [ForeignKey(nameof(ProductId))] public Product? Product { get; set; }

    [StringLength(100)] public string Label { get; set; } = null!;

    [StringLength(100)] public string IngredientType { get; set; } = null!;

    public double DefaultAmount { get; set; } = 0;

    public int? DisplayOrder { get; set; }

    [StringLength(20)] public string Unit { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; } = null!;

    public IEnumerable<AttributeOption>? AttributeOptions { get; set; } = new List<AttributeOption>();
}