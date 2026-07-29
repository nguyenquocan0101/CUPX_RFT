using Domain.Enums;

namespace Services.Dtos.Product;

public class ProductDto
{
    public string ProductId { get; set; } = null!;

    public string? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public ProductSize Size { get; set; }

    public ProductType Type { get; set; }

    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = null!;
}