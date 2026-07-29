using Services.Dtos.AttributeOption;

namespace Services.Dtos.ProductAttribute;

public class ProductAttributeInsideDto
{
    public string ProductAttributeId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string IngredientType { get; set; } = null!;

    public string? Description { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public double DefaultAmount { get; set; } = 0;

    public string Unit { get; set; } = null!;

    public IEnumerable<AttributeOptionInsideDto>? AttributeOptions { get; set; }
}