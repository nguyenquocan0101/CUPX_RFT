using Services.Dtos.LocationType;

namespace Services.Dtos.Store;

public class StoreReverseDto
{
    public string StoreId { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public string? ContactPhone { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? LocationAddress { get; set; } = null!;

    public string? LocationTypeId { get; set; } = null!;

    public virtual LocationTypeDto? LocationType { get; set; }

    public string Status { get; set; } = null!;
}