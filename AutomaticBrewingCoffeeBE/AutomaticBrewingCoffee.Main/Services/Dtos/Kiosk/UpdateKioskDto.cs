using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Kiosk;

public class UpdateKioskDto
{
    [StringLength(450)] public string? Position { get; set; } = null!;

    [StringLength(50)] public string? MenuId { get; set; }

    public DateTime? WarrantyTime { get; set; }

    [StringLength(50)] public string StoreId { get; set; } = null!;

    [StringLength(450)] public string Location { get; set; } = null!;

    [MatchEnum(typeof(EBaseStatus))] public string Status { get; set; } = null!;

    public DateTime InstalledDate { get; set; }

    [Required] public List<string> DeviceIds { get; set; } = new();
}