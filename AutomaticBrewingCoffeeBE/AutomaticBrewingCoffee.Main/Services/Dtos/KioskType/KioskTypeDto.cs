namespace Services.Dtos.KioskType;

public class KioskTypeDto
{
    public string KioskTypeId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;
    
    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; } = null!;
}