namespace Services.Dtos.LocationType;

public class LocationTypeDto
{
    public string LocationTypeId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; } = null!;
}