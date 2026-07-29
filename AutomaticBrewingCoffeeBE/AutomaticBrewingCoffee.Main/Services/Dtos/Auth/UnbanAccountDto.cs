using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Auth;

public class UnbanAccountDto
{
    [StringLength(50)] public string AccountId { get; set; } = null!;
    [StringLength(450)] public string UnbannedReason { get; set; } = null!;
}