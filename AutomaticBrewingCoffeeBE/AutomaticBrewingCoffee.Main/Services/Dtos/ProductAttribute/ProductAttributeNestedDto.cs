using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.AttributeOption;
using Services.Validations;

namespace Services.Dtos.ProductAttribute;

public class ProductAttributeNestedDto
{
    [StringLength(100)] public string Label { get; set; } = null!;

    [StringLength(100)]
    public string IngredientType { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public double DefaultAmount { get; set; } = 0;

    [StringLength(20)]
    [MatchEnum(typeof(EBaseUnit))]
    public string Unit { get; set; } = null!;

    public IEnumerable<AttributeOptionNestedDto>? AttributeOptions { get; set; }
}