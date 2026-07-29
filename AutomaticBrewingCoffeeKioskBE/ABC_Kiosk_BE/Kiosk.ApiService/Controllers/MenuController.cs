using AutomaticBrewingCoffee.API.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Dtos.Device;
using Services.Dtos.Menu;
using Services.Interfaces;

namespace Kiosk.ApiService.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/menus")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMenu([FromQuery] MenuQueryDto dto)
        {
            var result = await _menuService.GetMenus(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{menuId}")]
        public async Task<IActionResult> GetMenu(string menuId)
        {
            var result = await _menuService.GetMenu(menuId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenu(CreateMenuDto dto)
        {
            var result = await _menuService.CreateMenu(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{menuId}/products")]
        public async Task<IActionResult> AddProductToMenu(string menuId, CreateMenuProductMappingDto dto)
        {
            var result = await _menuService.AddProductToMenu(menuId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{menuId}")]
        public async Task<IActionResult> UpdateMenu(string menuId, UpdateMenuDto dto)
        {
            var result = await _menuService.UpdateMenu(menuId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{menuId}")]
        public async Task<IActionResult> DeleteDevice(string menuId)
        {
            var result = await _menuService.RemoveMenu(menuId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{menuId}/products/{productId}")]
        public async Task<IActionResult> RemoveProductOutMenu(string menuId, string productId)
        {
            var result = await _menuService.RemoveProductOutOfMenu(menuId, productId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
