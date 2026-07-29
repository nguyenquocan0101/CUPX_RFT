using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Kiosk;

public class UpdateKioskDto
{
    // DB: varchar(100), Nullable
    public string? DeviceId { get; set; } = null!;

    public string? FranchiseId { get; set; } = null!;

    // DB: varchar(300), Nullable
    [StringLength(300, ErrorMessage = "Location cannot exceed 300 characters.")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "InstalledDate is required.")]
    public DateTime? InstalledDate { get; set; }
    public BaseStatus Status { get; set; } = default!;
}