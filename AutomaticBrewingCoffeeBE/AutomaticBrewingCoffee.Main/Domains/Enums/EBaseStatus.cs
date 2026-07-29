using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EBaseStatus
{
    [Display(Name = "Active")] Active = 0,
    [Display(Name = "Inactive")] Inactive = 1
}