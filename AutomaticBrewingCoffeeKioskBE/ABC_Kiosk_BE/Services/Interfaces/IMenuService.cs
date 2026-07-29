using Domain.Pagination;
using Services.Base;
using Services.Dtos.Menu;

namespace Services.Interfaces;

public interface IMenuService
{
    Task<BaseResult<MenuQueryDto, Paginate<MenuDto>>> GetMenus(MenuQueryDto menuQueryDto);
    Task<BaseResult<string, MenuDto>> GetMenu(string menuId);
    Task<BaseResult<CreateMenuDto, MenuDto>> CreateMenu(CreateMenuDto createMenuDto);
    Task<BaseResult<UpdateMenuDto, MenuDto>> UpdateMenu(string menuId, UpdateMenuDto updateMenuDto);
    Task<BaseResult<string, MenuDto>> RemoveMenu(string menuId);
    Task<BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>> AddProductToMenu(string menuId, CreateMenuProductMappingDto createMenuProductMappingDto);
    Task<BaseResult<string, MenuProductMappingDto>> RemoveProductOutOfMenu(string menuId, string productId);
   
}