using Services.Dtos.Product;

namespace Services.Dtos.Menu
{
    public class MenuDto
    {
        public string MenuId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public virtual ICollection<MenuProductMappingDto> ProductsInMenu { get; set; } = new List<MenuProductMappingDto>();
    }
}
