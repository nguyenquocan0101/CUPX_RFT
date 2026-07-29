using Services.Dtos.Menu;
using Services.Dtos.Product;

namespace Services.Dtos.MenuProduct;

public class MenuProductMappingDto
{
    public string MenuId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public int? DisplayOrder { get; set; } = 0;

    public string? Status { get; set; }
    
    public decimal? SellingPrice { get; set; } = 0;

    public virtual ProductDto Product { get; set; } = null!;
    public virtual MenuInsideDto Menu { get; set; } = null!;
}