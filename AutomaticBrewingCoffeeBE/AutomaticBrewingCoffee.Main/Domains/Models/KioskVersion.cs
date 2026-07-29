using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class KioskVersion : BaseModel
{
    [Key] [StringLength(50)] [Required] public string KioskVersionId { get; set; } = null!;

    [StringLength(50)] public string? KioskTypeId { get; set; }

    public virtual KioskType? KioskType { get; set; }

    [StringLength(100)] public string VersionTitle { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(50)] public string VersionNumber { get; set; } = null!;

    [StringLength(50)] public string Status { get; set; } = null!;

    public virtual List<Kiosk>? Kiosks { get; set; } = null!;

    public virtual IEnumerable<KioskVersionDeviceModelMapping> KioskVersionDeviceModelMappings { get; set; } =
        new List<KioskVersionDeviceModelMapping>();

    public virtual IEnumerable<KioskVersionProductMapping> KioskVersionProductMappings { get; set; } =
        new List<KioskVersionProductMapping>();
}