using Services.Dtos.Product;

namespace Services.Dtos.MenuProduct;

public class MenuProductMappingForKioskDto
{
    public string MenuId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public int? DisplayOrder { get; set; } = 0;

    public string? StatusInMenu { get; set; }

    public decimal? SellingPrice { get; set; } = 0;

    public virtual ProductForKioskDto Product { get; set; } = null!;

    public bool IsAvailable { get; set; } = true;
}