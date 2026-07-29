namespace Services.Dtos.DeviceType;

public class DeviceTypeDto
{
    public string DeviceTypeId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;
    
    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; } = null!;
    
    public bool IsMobileDevice { get; set; }
}