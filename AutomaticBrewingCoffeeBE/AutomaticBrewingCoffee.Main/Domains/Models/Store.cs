using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Store : BaseModel
{
    [Key] [StringLength(50)] [Required] public string StoreId { get; set; } = null!;

    [StringLength(50)] [Required] public string OrganizationId { get; set; } = null!;

    [ForeignKey(nameof(OrganizationId))] public virtual Organization? Organization { get; set; }

    [StringLength(100)] public string? ContactPhone { get; set; }

    [StringLength(100)] public string? Name { get; set; }

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(450)] public string? LocationAddress { get; set; } = null!;

    [StringLength(50)] public string? LocationTypeId { get; set; } = null!;

    [ForeignKey(nameof(LocationTypeId))] public virtual LocationType? LocationType { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    // Navigation property
    public virtual ICollection<Kiosk> Kiosks { get; set; } = new List<Kiosk>();
}