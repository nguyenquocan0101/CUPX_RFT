using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EParameterType
{
    [Display(Name = "Integer")] Integer,
    [Display(Name = "Double")] Double,
    [Display(Name = "Boolean")] Boolean,
    [Display(Name = "Text")] Text,
}