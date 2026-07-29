using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Organization : BaseModel
{
    [Key] [StringLength(50)] [Required] public string OrganizationId { get; set; } = null!;
    [StringLength(100)] [Required] public string Name { get; set; } = null!;
    [StringLength(100)] public string OrganizationCode { get; set; } = null!;
    [StringLength(450)] public string? Description { get; set; }
    [StringLength(100)] public string? ContactPhone { get; set; }
    [StringLength(100)] public string? ContactEmail { get; set; }

    [StringLength(450)] public string? LogoUrl { get; set; }
    [StringLength(200)] public string? TaxId { get; set; }
    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}