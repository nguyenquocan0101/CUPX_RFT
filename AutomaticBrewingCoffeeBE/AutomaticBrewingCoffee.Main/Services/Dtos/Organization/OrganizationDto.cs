namespace Services.Dtos.Organization;

public class OrganizationDto
{
    public string OrganizationId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string OrganizationCode { get; set; } = null!;
    public string? Description { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? LogoUrl { get; set; }
    public string? TaxId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; } = null!;
}