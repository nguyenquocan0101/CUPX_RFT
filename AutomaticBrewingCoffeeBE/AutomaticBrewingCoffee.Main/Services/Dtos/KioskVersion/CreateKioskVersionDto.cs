using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.KioskType;
using Services.Validations;

namespace Services.Dtos.KioskVersion;

public class CreateKioskVersionDto
{
    [StringLength(50)] public string? KioskTypeId { get; set; }

    [StringLength(100)] public string VersionTitle { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(50)] public string VersionNumber { get; set; } = null!;

    [MatchEnum(typeof(EBaseStatus))] public string Status { get; set; } = null!;
}