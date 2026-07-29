namespace Services.Dtos.Product;

public class ProductNestedDto
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

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}