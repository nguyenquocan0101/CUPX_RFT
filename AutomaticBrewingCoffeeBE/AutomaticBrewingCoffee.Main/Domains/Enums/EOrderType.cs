using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EOrderType
{
    [Display(Name = "Immediate")] Immediate = 0,

    [Display(Name = "PreOrder")] PreOrder = 1
}