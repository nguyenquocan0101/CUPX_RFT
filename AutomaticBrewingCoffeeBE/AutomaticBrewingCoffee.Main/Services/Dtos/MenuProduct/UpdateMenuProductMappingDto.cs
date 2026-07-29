using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.MenuProduct;

public class UpdateMenuProductMappingDto
{
    [StringLength(10)]
    [Required]
    [MatchEnum(typeof(EBaseStatus))]
    public string Status { get; set; } = null!;

    public decimal? SellingPrice { get; set; }
}