using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.MenuProduct;

public class CreateMenuProductMappingDto
{
    [StringLength(50)] [Required] public string MenuId { get; set; } = null!;

    [StringLength(50)] [Required] public string ProductId { get; set; } = null!;

    [StringLength(10)]
    [Required]
    [MatchEnum(typeof(EBaseStatus))]
    public string Status { get; set; } = null!;

    public decimal? SellingPrice { get; set; }
}