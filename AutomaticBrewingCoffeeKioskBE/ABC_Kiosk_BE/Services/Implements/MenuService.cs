using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Domain.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Base;
using Services.Dtos.Menu;
using Services.Dtos.Product;
using Services.Interfaces;
using Services.Utils;
using System.Linq.Expressions;

namespace Services.Implements;

public class MenuService : BaseService<MenuService>, IMenuService
{
    public MenuService
    (
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor
    ) : base(unitOfWork,mapper,loggerFactory,httpContextAccessor)
    {
    }

    public async Task<BaseResult<MenuQueryDto, Paginate<MenuDto>>> GetMenus(MenuQueryDto menuQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetMenus", menuQueryDto);

        var predicate = _unitOfWork.GetRepository<Menu>()
            .BuildSearchPredicate(menuQueryDto.FilterQuery, menuQueryDto.FilterBy);

        if (menuQueryDto.Status is not null)
        {
            Expression<Func<Menu, bool>> statusFilter = x => x.Status == menuQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions<Menu>(predicate, statusFilter);
        }

        var orderBy = _unitOfWork.GetRepository<Menu>()
            .BuildSortingQuery(menuQueryDto.SortBy, menuQueryDto.IsAsc);

        var menus = await _unitOfWork.GetRepository<Menu>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            include: q => q.Include(m => m.MenuProductMappings).ThenInclude(mpm => mpm.Product),
            page: menuQueryDto.Page,
            size: menuQueryDto.Size
        );
        //filter only product for menu
        var menuDtos = _mapper.Map<Paginate<MenuDto>>(menus);
        foreach (var menu in menuDtos.Items)
        {
            var menuProductList = await _unitOfWork.GetRepository<MenuProductMapping>().GetListAsync(
                predicate: x => x.MenuId.Equals(menu.MenuId),
                selector: x => _mapper.Map<MenuProductMappingDto>(x),
                include: q => q.Include(m => m.Product));

            menu.ProductsInMenu = menuProductList;
        }

        LogMessage(LogLevel.Information, "Out GetMenus", menuDtos);

        return new BaseResult<MenuQueryDto, Paginate<MenuDto>>()
        {
            IsSuccess = true,
            Message = "Menus found.",
            Request = menuQueryDto,
            Response = menuDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, MenuDto>> GetMenu(string menuId)
    {
        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId,
            include: q => q.Include(x => x.MenuProductMappings).ThenInclude(x => x.Product)
        );

        if (menu is null)
        {
            return new BaseResult<string, MenuDto>()
            {
                IsSuccess = false,
                Message = "Menu not found",
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = menuId
            };
        }

        var menuDto = _mapper.Map<MenuDto>(menu);

        return new BaseResult<string, MenuDto>()
        {
            IsSuccess = true,
            Message = "Menu found",
            Request = menuId,
            Response = menuDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateMenuDto, MenuDto>> CreateMenu(CreateMenuDto createMenuDto)
    {
        LogMessage(LogLevel.Information, "In CreateDevice", createMenuDto);

        var menu = new Menu
        {
            MenuId = Guid.NewGuid().ToString(),
            Name = createMenuDto.Name,
            Description = createMenuDto.Description,
            Status = createMenuDto.Status
        };

        await _unitOfWork.GetRepository<Menu>().InsertAsync(menu);
        var result = await _unitOfWork.CommitAsync() > 0;

        LogMessage(LogLevel.Information, "Insert Device", result);

        var deviceDto = _mapper.Map<MenuDto>(menu);

        LogMessage(LogLevel.Information, "Out CreateDevice", deviceDto);

        return new BaseResult<CreateMenuDto, MenuDto>
        {
            IsSuccess = result,
            Message = "Device created successfully.",
            Request = createMenuDto,
            Response = deviceDto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateMenuDto, MenuDto>> UpdateMenu(string menuId, UpdateMenuDto updateMenuDto)
    {
        LogMessage(LogLevel.Information, "In UpdateMenu", updateMenuDto);

        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId
        );

        if (menu is null)
        {
            return new BaseResult<UpdateMenuDto, MenuDto>()
            {
                IsSuccess = false,
                Message = "Menu not found",
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = updateMenuDto
            };
        }


        menu = _mapper.Map(updateMenuDto, menu);

        _unitOfWork.GetRepository<Menu>().Update(menu);

        var result = await _unitOfWork.CommitAsync();

        LogMessage(LogLevel.Information, "Update Device", result);

        var menuDto = _mapper.Map<MenuDto>(menu);

        LogMessage(LogLevel.Information, "Out UpdateMenu", updateMenuDto);

        return new BaseResult<UpdateMenuDto, MenuDto>()
        {
            IsSuccess = true,
            Message = "Menu updated",
            StatusCode = StatusCodes.Status202Accepted,
            Response = menuDto,
            Request = updateMenuDto
        };
    }

    public async Task<BaseResult<string, MenuDto>> RemoveMenu(string menuId)
    {
        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId
        );

        if (menu is null)
        {
            return new BaseResult<string, MenuDto>()
            {
                IsSuccess = false,
                Message = "Menu not found",
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = menuId
            };
        }

        _unitOfWork.GetRepository<Menu>().Delete(menu);
        _unitOfWork.Commit();

        var menuDto = _mapper.Map<MenuDto>(menu);

        return new BaseResult<string, MenuDto>()
        {
            IsSuccess = true,
            Message = "",
            StatusCode = StatusCodes.Status202Accepted,
            Response = menuDto,
            Request = menuId
        };
    }

    public async Task<BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>> AddProductToMenu(string menuId, CreateMenuProductMappingDto createMenuProductMappingDto)
    {
        //check product already in menu or not
        var productInMenu = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
            predicate: x => x.ProductId.Equals(createMenuProductMappingDto.ProductId) && x.MenuId.Equals(menuId)
        );
        if (productInMenu != null)
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = "Product already added",
                StatusCode = StatusCodes.Status400BadRequest,
                Request = createMenuProductMappingDto,
                Response = null
            };
        
        var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
            predicate: x => x.ProductId == createMenuProductMappingDto.ProductId
        );

        if (product is null)
        {
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = "Product not found",
                StatusCode = StatusCodes.Status404NotFound,
                Request = createMenuProductMappingDto,
                Response = null
            };
        }

        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId
        );

        if (menu is null)
        {
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = "Menu not found",
                StatusCode = StatusCodes.Status404NotFound,
                Request = createMenuProductMappingDto,
                Response = null
            };
        }

        var total = await _unitOfWork.GetRepository<MenuProductMapping>().GetListAsync();
        var menuProductMapping = new MenuProductMapping
        {
            MenuId = menuId,
            ProductId = createMenuProductMappingDto.ProductId,
            DisplayOrder = createMenuProductMappingDto.DisplayOrder,
            Status = createMenuProductMappingDto.Status
        };

        await _unitOfWork.GetRepository<MenuProductMapping>().InsertAsync(menuProductMapping);
        var isSuccess = await _unitOfWork.CommitAsync() > 0;

        var menuProductMappingDto = _mapper.Map<MenuProductMappingDto>(menuProductMapping);

        return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
        {
            IsSuccess = isSuccess,
            Message = "MenuProductMapping created",
            StatusCode = StatusCodes.Status201Created,
            Request = createMenuProductMappingDto,
            Response = menuProductMappingDto
        };
    }

    public async Task<BaseResult<string, MenuProductMappingDto>> RemoveProductOutOfMenu(string menuId, string productId)
    {
        var menuProductMapping = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId && x.ProductId == productId
        );

        if (menuProductMapping is null)
        {
            return new BaseResult<string, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = "MenuProductMapping not found",
                Request = $"{menuId} {productId}",
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        _unitOfWork.GetRepository<MenuProductMapping>().Delete(menuProductMapping);
        _unitOfWork.Commit();

        var menuProductMappingDto = _mapper.Map<MenuProductMappingDto>(menuProductMapping);

        return new BaseResult<string, MenuProductMappingDto>()
        {
            IsSuccess = true,
            Message = "MenuProductMapping deleted",
            Request = $"{menuId} {productId}",
            Response = menuProductMappingDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

}