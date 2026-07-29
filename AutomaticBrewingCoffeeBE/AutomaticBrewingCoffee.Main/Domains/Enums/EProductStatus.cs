using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EProductStatus
{
    [Display(Name = "Selling")] Selling = 0,

    [Display(Name = "UnSelling")] UnSelling = 1,
}