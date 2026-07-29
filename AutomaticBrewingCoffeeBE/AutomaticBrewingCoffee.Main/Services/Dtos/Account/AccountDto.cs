using Services.Dtos.Organization;

namespace Services.Dtos.Account;

public class AccountDto
{
    public string AccountId { get; set; } = null!;

    public string? FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool IsBanned { get; set; } = false;

    public string? BannedReason { get; set; }

    public string? OrganizationId { get; set; } = null!;

    public OrganizationDto? Organization { get; set; }
}