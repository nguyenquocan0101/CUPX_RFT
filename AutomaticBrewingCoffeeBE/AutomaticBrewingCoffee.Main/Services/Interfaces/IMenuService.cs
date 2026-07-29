using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Menu;
using Services.Dtos.MenuProduct;

namespace Services.Interfaces;

public interface IMenuService
{
    Task<BaseResult<MenuQueryDto, Paginate<MenuDto>>> GetMenus(MenuQueryDto menuQueryDto);

    Task<BaseResult<string, MenuDto>> GetMenu(string menuId);

    Task<BaseResult<string, MenuForKioskDto>> GetMenuForKiosk();

    Task<BaseResult<CreateMenuDto, MenuDto>> CreateMenu(CreateMenuDto createMenuDto);

    Task<BaseResult<CloneMenuDto, MenuDto>> CloneMenu(CloneMenuDto cloneMenuDto);

    Task<BaseResult<UpdateMenuDto, MenuDto>> UpdateMenu(string menuId, UpdateMenuDto updateMenuDto);

    Task<BaseResult<string, MenuDto>> RemoveMenu(string menuId);

    Task<BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>> CreateMenuProductMapping(
        CreateMenuProductMappingDto createMenuProductMappingDto);

    Task<BaseResult<UpdateMenuProductMappingDto, MenuProductMappingDto>> UpdateMenuProductMapping(
        string menuId, string productId, UpdateMenuProductMappingDto updateMenuProductMappingDto);

    Task<BaseResult<MenuProductMappingQueryDto, Paginate<MenuProductMappingDto>>> GetMenuProductMappings(
        MenuProductMappingQueryDto menuProductMappingQueryDto);

    Task<BaseResult<string, MenuProductMappingDto>> GetMenuProductMapping(
        string menuId, string productId);

    Task<BaseResult<string, MenuProductMappingDto>> RemoveMenuProductMapping(
        string menuId, string productId);

    Task<BaseResult<ReorderMenuProductMappingDto, List<MenuProductMappingDto>>> ReorderMenuProductAsync(
        string menuId,
        ReorderMenuProductMappingDto reorderMenuProductMappingDto
    );
}