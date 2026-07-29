using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.AttributeOption;

public class AttributeOptionNestedDto
{
    [StringLength(100)] public string Name { get; set; } = null!;

    public double Value { get; set; }

    [StringLength(20)]
    [MatchEnum(typeof(EAttributeOptionUnit))]
    public string Unit { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool IsDefault { get; set; }
    
    [StringLength(450)] public string? Description { get; set; } = null!;
}