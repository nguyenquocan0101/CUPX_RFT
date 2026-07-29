using Services.Dtos.Kiosk;
using Services.Dtos.LocationType;
using Services.Dtos.Organization;

namespace Services.Dtos.Store;

public class StoreDto
{
    public string StoreId { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public virtual OrganizationDto? Organization { get; set; }

    public string? ContactPhone { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? LocationAddress { get; set; } = null!;

    public string? LocationTypeId { get; set; } = null!;

    public virtual LocationTypeDto? LocationType { get; set; }

    public string Status { get; set; } = null!;

    // Navigation property
    public virtual List<KioskDto> Kiosks { get; set; } = new List<KioskDto>();
}