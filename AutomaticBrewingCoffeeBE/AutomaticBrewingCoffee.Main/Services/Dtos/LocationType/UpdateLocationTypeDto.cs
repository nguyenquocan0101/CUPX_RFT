using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.LocationType;

public class UpdateLocationTypeDto
{
    [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(200)] public string? Description { get; set; } = string.Empty;
}