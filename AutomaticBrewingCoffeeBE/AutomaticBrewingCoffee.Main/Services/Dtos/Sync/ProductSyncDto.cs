namespace Services.Dtos.Sync;

public class ProductSyncDto
{
    public string ProductId { get; set; } = null!;

    public string? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string? Size { get; set; }

    public string? Type { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }
}