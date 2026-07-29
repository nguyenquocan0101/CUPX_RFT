using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.KioskType;

public class UpdateKioskTypeDto
{
    [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(200)] public string Description { get; set; } = null!;

    [StringLength(10)] [Required] [MatchEnum(typeof(EBaseStatus))] public string Status { get; set; } = null!;
}