using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Organization;

public class CreateOrganizationDto
{
    [StringLength(100)] [Required] public string Name { get; set; } = null!;
    [StringLength(450)] public string? Description { get; set; }

    [StringLength(100)]
    [Required]
    [PhoneVN]
    public string ContactPhone { get; set; } = null!;

    [StringLength(100)]
    [Required]
    [MatchEmail]
    public string ContactEmail { get; set; } = null!;

    [MatchBase64] public string? LogoBase64 { get; set; }
    public string? LogoUrl { get; set; }
    [StringLength(200)] public string? TaxId { get; set; }

    [StringLength(10)]
    [Required]
    [MatchEnum(typeof(EBaseStatus))]
    public string Status { get; set; } = null!;
}