using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

[Index(nameof(Email), IsUnique = true)]
public class Account : BaseModel
{
    [Key] [Required] [StringLength(50)] public string AccountId { get; set; } = null!;
    [Required] [StringLength(100)] public string? FullName { get; set; } = null!;
    [Required] [StringLength(150)] public string Email { get; set; } = null!;
    [Required] [StringLength(150)] public string Password { get; set; } = null!;
    [Required] [StringLength(100)] public string RoleName { get; set; } = null!;

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    public bool IsBanned { get; set; } = false;

    [StringLength(450)] public string? BannedReason { get; set; }

    [StringLength(450)] public string? UnbannedReason { get; set; }

    [StringLength(50)] public string? OrganizationId { get; set; } = null!;

    [ForeignKey(nameof(OrganizationId))] public Organization? Organization { get; set; } = null!;

    [StringLength(1000)] public string? RefreshToken { get; set; } = null!;


    public void Ban(string banReason)
    {
        BannedReason = banReason;
        IsBanned = true;
        UpdatedDate = DateTime.UtcNow;
        RefreshToken = null;
    }

    public void Unban(string unbanReason)
    {
        UnbannedReason = unbanReason;
        BannedReason = null;
        IsBanned = false;
        UpdatedDate = DateTime.UtcNow;
    }
}