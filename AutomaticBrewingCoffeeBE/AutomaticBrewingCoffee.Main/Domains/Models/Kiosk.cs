using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Kiosk : BaseModel
{
    [Key] [StringLength(50)] [Required] public string KioskId { get; set; } = null!;

    [StringLength(50)] public string? KioskVersionId { get; set; }
    
    [StringLength(50)] public string? MenuId { get; set; }

    [StringLength(250)] public string? ApiKey { get; set; }

    [StringLength(250)] public bool IsRevoke { get; set; } = false;

    [StringLength(450)] public string? Hostname { get; set; }

    [StringLength(450)] public string? OriginServer { get; set; }

    [ForeignKey(nameof(KioskVersionId))] public virtual KioskVersion? KioskVersion { get; set; }

    [StringLength(450)] public string? Position { get; set; } = null!;

    public DateTime? WarrantyTime { get; set; }

    [StringLength(50)] [Required] public string StoreId { get; set; } = null!;

    [ForeignKey(nameof(StoreId))] public virtual Store? Store { get; set; } = null!;

    [StringLength(450)] [Required] public string Location { get; set; } = null!;

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    public DateTime InstalledDate { get; set; }

    public virtual ICollection<KioskDeviceMapping> KioskDevices { get; set; } = new List<KioskDeviceMapping>();
}