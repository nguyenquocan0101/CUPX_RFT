using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Menu;
using Services.Dtos.MenuProduct;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/menus")]
    [ApiController]
    [TrimStrings]
    public class MenusController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // GET: api/menus
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of menus",
            Description =
                "Retrieve a paginated list of menu items with optional filters such as category, status, or keyword."
        )]
        public async Task<ActionResult<BaseResult<MenuQueryDto, Paginate<MenuDto>>>> Get(
            [FromQuery] MenuQueryDto menuQueryDto)
        {
            var response = await _menuService.GetMenus(menuQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/menus/{menuId}
        [HttpGet("{menuId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get menu item details",
            Description = "Retrieve detailed information about a specific menu item by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, MenuDto>>> Get(string menuId)
        {
            var response = await _menuService.GetMenu(menuId);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/menus/{menuId}
        [HttpGet("by-kiosk")]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Get menu item details",
            Description = "Retrieve detailed information about a specific menu item by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, MenuDto>>> Get()
        {
            var response = await _menuService.GetMenuForKiosk();
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/menus
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new menu item",
            Description = "Create a new menu item with details such as name, price, description, and category."
        )]
        public async Task<ActionResult<BaseResult<CreateMenuDto, MenuDto>>> Post(
            [FromBody] CreateMenuDto createMenuDto)
        {
            var response = await _menuService.CreateMenu(createMenuDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT: api/menus/{menuId}
        [HttpPut("{menuId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Update menu item",
            Description = "Update the details of an existing menu item by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateMenuDto, MenuDto>>> Put(
            string menuId,
            [FromBody] UpdateMenuDto updateMenuDto)
        {
            var response = await _menuService.UpdateMenu(menuId, updateMenuDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE: api/menus/{menuId}
        [HttpDelete("{menuId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a menu item",
            Description = "Delete an existing menu item by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, MenuDto>>> Delete(string menuId)
        {
            var response = await _menuService.RemoveMenu(menuId);
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/menus/product
        [HttpPost("menu-products")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Add product mapping to menu",
            Description = "Associate a product with a menu item using mapping data such as product ID and quantity."
        )]
        public async Task<ActionResult<BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>>>
            CreateMenuProductMapping(
                [FromBody] CreateMenuProductMappingDto createMenuProductMappingDto)
        {
            var response = await _menuService.CreateMenuProductMapping(createMenuProductMappingDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/menus/{menuId}/products/{productId}/mapping
        [HttpGet("{menuId}/menu-products/{productId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get menu-product mapping details",
            Description = "Retrieve the mapping details between a specific menu and product by their IDs."
        )]
        public async Task<ActionResult<BaseResult<string, MenuProductMappingDto>>> GetMenuProductMapping(
            [FromRoute] string menuId,
            [FromRoute] string productId)
        {
            var response = await _menuService.GetMenuProductMapping(menuId, productId);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/menus/menu-products
        [HttpGet("menu-products")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of menu-product mappings",
            Description =
                "Retrieve a paginated list of menu-product mappings with optional filters such as menu ID or product ID."
        )]
        public async Task<ActionResult<BaseResult<MenuProductMappingQueryDto, Paginate<MenuProductMappingDto>>>>
            GetMenuProductMappings(
                [FromQuery] MenuProductMappingQueryDto queryDto)
        {
            var response = await _menuService.GetMenuProductMappings(queryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{menuId}/menu-products/{productId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Update a product from a menu",
            Description = "Update the mapping between a specific product and menu using their IDs."
        )]
        public async Task<ActionResult<BaseResult<UpdateMenuProductMappingDto, MenuProductMappingDto>>>
            UpdateMenuProductMapping(
                [FromRoute] string menuId,
                [FromRoute] string productId,
                [FromBody] UpdateMenuProductMappingDto updateMenuProductMappingDto
            )
        {
            var response = await _menuService.UpdateMenuProductMapping(menuId, productId, updateMenuProductMappingDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{menuId}/menu-products/{productId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Remove a product from a menu",
            Description = "Remove the mapping between a specific product and menu using their IDs."
        )]
        public async Task<ActionResult<BaseResult<string, MenuProductMappingDto>>> RemoveMenuProductMapping(
            [FromRoute] string menuId,
            [FromRoute] string productId)
        {
            var response = await _menuService.RemoveMenuProductMapping(menuId, productId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{menuId}/menu-products/reorder")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        public async Task<ActionResult<BaseResult<ReorderMenuProductMappingDto, List<MenuProductMappingDto>>>>
            ReorderMenuProduct(
                [FromRoute] string menuId,
                [FromBody] ReorderMenuProductMappingDto reorderMenuProductMappingDto
            )
        {
            var result = await _menuService.ReorderMenuProductAsync(menuId, reorderMenuProductMappingDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("clone")]
        [SwaggerOperation(
            Summary = "Clone a new menu",
            Description = "Clone the mapping between a specific product and menu using their IDs."
        )]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        public async Task<ActionResult<BaseResult<CloneMenuDto, MenuDto>>>
            CloneMenu(
                [FromBody] CloneMenuDto cloneMenuDto
            )
        {
            var result = await _menuService.CloneMenu(cloneMenuDto);
            return StatusCode(result.StatusCode, result);
        }
    }
}