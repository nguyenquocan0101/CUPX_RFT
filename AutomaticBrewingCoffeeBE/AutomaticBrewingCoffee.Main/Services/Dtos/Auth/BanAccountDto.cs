using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Auth;

public class BanAccountDto
{
    [StringLength(50)] public string AccountId { get; set; } = null!;

    [StringLength(450)] public string BannedReason { get; set; } = null!;
}