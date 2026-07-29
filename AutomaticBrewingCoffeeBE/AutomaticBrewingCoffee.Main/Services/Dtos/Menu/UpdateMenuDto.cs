using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Menu;

public class UpdateMenuDto
{
    [StringLength(50)] public string OrganizationId { get; set; } = null!;

    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(10)]
    [MatchEnum(typeof(EBaseStatus))]
    public string Status { get; set; } = null!;
}