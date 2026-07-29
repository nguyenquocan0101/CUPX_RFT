using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;

namespace AutomaticBrewingCoffee.Domain.Models;

public class AttributeOption : BaseModel
{
    [Key] [StringLength(50)] public string AttributeOptionId { get; set; } = null!;

    [StringLength(50)] public string ProductAttributeId { get; set; } = null!;

    [ForeignKey(nameof(ProductAttributeId))]
    public ProductAttribute? ProductAttribute { get; set; }

    [StringLength(100)] public string Name { get; set; } = null!;


    // the percent of ingredient base on default in productAttribute
    public double Value { get; set; }

    public int? DisplayOrder { get; set; }

    public bool IsDefault { get; set; }

    [StringLength(20)] public string Unit { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; } = null!;
}