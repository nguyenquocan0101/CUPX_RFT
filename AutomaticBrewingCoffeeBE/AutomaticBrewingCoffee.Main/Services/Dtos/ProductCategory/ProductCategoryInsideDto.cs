namespace Services.Dtos.ProductCategory;

public class ProductCategoryInsideDto
{
    public string ProductCategoryId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string? ImageUrl { get; set; }
    
    public int? DisplayOrder { get; set; } = 0;
}