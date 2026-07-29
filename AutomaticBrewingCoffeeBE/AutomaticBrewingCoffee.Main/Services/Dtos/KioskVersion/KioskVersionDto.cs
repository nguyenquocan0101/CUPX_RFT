using System.ComponentModel.DataAnnotations;
using Services.Dtos.Kiosk;
using Services.Dtos.KioskType;
using Services.Dtos.KioskVersionDeviceModel;
using Services.Dtos.KioskVersionProduct;

namespace Services.Dtos.KioskVersion;

public class KioskVersionDto
{
    public string KioskVersionId { get; set; } = null!;

    public string? KioskTypeId { get; set; }

    public virtual KioskTypeDto? KioskType { get; set; }

    [StringLength(100)] public string VersionTitle { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(50)] public string VersionNumber { get; set; } = null!;

    [StringLength(50)] public string Status { get; set; } = null!;

    public virtual List<KioskDto>? Kiosks { get; set; } = null!;

    public virtual List<KioskVersionDeviceModelInsideDto>? KioskVersionDeviceModelMappings { get; set; } = null!;

    public virtual List<KioskVersionProductInsideDto>? KioskVersionProductMappings { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; } = null!;
}