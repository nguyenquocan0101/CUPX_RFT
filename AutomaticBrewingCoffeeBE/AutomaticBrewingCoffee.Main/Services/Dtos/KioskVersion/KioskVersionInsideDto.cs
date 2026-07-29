using Services.Dtos.KioskType;

namespace Services.Dtos.KioskVersion;

public class KioskVersionInsideDto
{
    public string KioskVersionId { get; set; } = null!;

    public string? KioskTypeId { get; set; }

    public virtual KioskTypeDto? KioskType { get; set; }

    public string VersionTitle { get; set; } = null!;

    public string? Description { get; set; }

    public string VersionNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; } = null!;
}