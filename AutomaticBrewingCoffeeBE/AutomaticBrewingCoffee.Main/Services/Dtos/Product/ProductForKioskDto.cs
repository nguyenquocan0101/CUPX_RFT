using Services.Dtos.ProductAttribute;
using Services.Dtos.ProductCategory;

namespace Services.Dtos.Product;

public class ProductForKioskDto
{
    public string ProductId { get; set; } = null!;

    public string? TagName { get; set; }

    public string? ParentId { get; set; }

    public string? ProductCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Size { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? ProductParentName { get; set; } = null!;

    public ProductCategoryInsideDto? ProductCategory { get; set; }

    public List<ProductAttributeInsideDto>? ProductAttributes { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}