namespace Services.Dtos.AttributeOption;

public class AttributeOptionInsideDto
{
    public string AttributeOptionId { get; set; } = null!;

    public string ProductAttributeId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public double Value { get; set; }

    public string Unit { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool IsDefault { get; set; }

    public string? Description { get; set; } = null!;
}