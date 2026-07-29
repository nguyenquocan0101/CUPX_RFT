using Services.Dtos.Kiosk;
using Services.Dtos.MenuProduct;
using Services.Dtos.Organization;

namespace Services.Dtos.Menu
{
    public class MenuDto
    {
        public string MenuId { get; set; } = null!;

        public string OrganizationId { get; set; } = null!;

        public OrganizationDto? Organization { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string Status { get; set; } = null!;

        public virtual ICollection<MenuProductMappingDto> MenuProductMappings { get; set; } =
            new List<MenuProductMappingDto>();
    }
}