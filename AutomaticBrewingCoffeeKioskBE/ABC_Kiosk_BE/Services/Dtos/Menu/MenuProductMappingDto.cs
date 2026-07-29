using Services.Dtos.Product;

namespace Services.Dtos.Menu
{
    public class MenuProductMappingDto
    {
        public string MenuId { get; set; } = null!;

        public string ProductId { get; set; } = null!;

        public int? DisplayOrder { get; set; } = 0;

        public string? StatusInMenu { get; set; }

        public virtual ProductDto Product { get; set; } = null!;
    }
}
