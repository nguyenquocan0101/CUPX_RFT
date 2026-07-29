using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Order;

public class ChangeOrderStatusDto
{
    [Required]
    [MatchEnum(typeof(EOrderStatus))]
    public string Status { get; set; } = null!;
}