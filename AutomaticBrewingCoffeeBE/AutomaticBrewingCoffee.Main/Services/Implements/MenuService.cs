using System.Linq.Expressions;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Services.Base;
using Services.Dtos.Menu;
using Services.Dtos.MenuProduct;
using Services.Dtos.Organization;
using Services.Interfaces;
using Services.Utils;

namespace Services.Implements;

public class MenuService : BaseService<MenuService>, IMenuService
{
    public MenuService
    (
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor
    ) : base(
        unitOfWork,
        mapper,
        loggerFactory,
        httpContextAccessor
    )
    {
    }

    public async Task<BaseResult<MenuQueryDto, Paginate<MenuDto>>> GetMenus(MenuQueryDto menuQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetMenus", menuQueryDto);

        var predicate = _unitOfWork.GetRepository<Menu>()
            .BuildSearchPredicate(menuQueryDto.FilterQuery, menuQueryDto.FilterBy);

        Expression<Func<Menu, bool>> isDeletedFilter = x =>
            x.IsDeleted == false;
        predicate = ExpressionHelper.CombineExpressions<Menu>(predicate, isDeletedFilter);

        if (menuQueryDto.StartDate is not null && menuQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<Menu>().BuildDateRangePredicate(
                menuQueryDto.StartDate,
                menuQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        var roles = GetAccountRolesFromJwt();

        if (roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            menuQueryDto.OrganizationId = referenceId;
        }

        if (menuQueryDto.Status is not null)
        {
            Expression<Func<Menu, bool>> statusFilter = x => x.Status == menuQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions<Menu>(predicate, statusFilter);
        }

        if (menuQueryDto.OrganizationId is not null)
        {
            Expression<Func<Menu, bool>> franchiseFilter = x => x.OrganizationId == menuQueryDto.OrganizationId;
            predicate = ExpressionHelper.CombineExpressions<Menu>(predicate, franchiseFilter);
        }

        var orderBy = _unitOfWork.GetRepository<Menu>()
            .BuildSortingQuery(menuQueryDto.SortBy, menuQueryDto.IsAsc);

        var menus = await _unitOfWork.GetRepository<Menu>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: menuQueryDto.Page,
            size: menuQueryDto.Size,
            include: x => x.Include(x => x.Organization)
        );

        var menuDtos = _mapper.Map<Paginate<MenuDto>>(menus);

        LogMessage(LogLevel.Information, "Out GetMenus", menuDtos);

        return new BaseResult<MenuQueryDto, Paginate<MenuDto>>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Menu>(),
            Request = menuQueryDto,
            Response = menuDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, MenuDto>> GetMenu(string menuId)
    {
        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId,
            include: x =>
                x.Include(x => x.MenuProductMappings)
                    .ThenInclude(x => x.Product).ThenInclude(x => x.ProductCategory)
                    .Include(x => x.Organization)
        );

        if (menu is null)
        {
            return new BaseResult<string, MenuDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Menu>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = menuId
            };
        }


        var menuDto = _mapper.Map<MenuDto>(menu);

        var kiosk = await _unitOfWork.GetRepository<Organization>()
            .SingleOrDefaultAsync(predicate: x => x.OrganizationId == menu.OrganizationId);
        var organizationDto = _mapper.Map<OrganizationDto>(kiosk);
        menuDto.Organization = organizationDto;

        return new BaseResult<string, MenuDto>()
        {
            IsSuccess = false,
            Message = MessageUtil.ReadSuccess<Menu>(),
            Request = menuId,
            Response = menuDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, MenuForKioskDto>> GetMenuForKiosk()
    {
        var kioskId = GetKioskIdFromJwt();

        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kioskId,
            include: x =>
                x.Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
        );

        if (kiosk is null)
        {
            return new BaseResult<string, MenuForKioskDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Kiosk>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = kioskId
            };
        }

        if (kiosk.MenuId is null)
        {
            return new BaseResult<string, MenuForKioskDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Menu>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = kiosk.MenuId
            };
        }

        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == kiosk.MenuId,
            include: x => x.Include(x => x.MenuProductMappings)
                .ThenInclude(x => x.Product)
                .ThenInclude(p => p.ProductCategory)
                .Include(x => x.MenuProductMappings)
                .ThenInclude(x => x.Product)
                .ThenInclude(p => p.ProductAttributes)
                .ThenInclude(pa => pa.AttributeOptions)
        );

        if (menu is null)
        {
            return new BaseResult<string, MenuForKioskDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Menu>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = kiosk.MenuId
            };
        }

        if (menu.Status == EBaseStatus.Inactive.ToString())
        {
            return new BaseResult<string, MenuForKioskDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.IsInactive<Menu>(),
                Request = kiosk.MenuId,
                Response = null,
                StatusCode = StatusCodes.Status423Locked
            };
        }

        if (menu.MenuProductMappings != null && menu.MenuProductMappings.Count != 0)
        {
            menu.MenuProductMappings =
                menu.MenuProductMappings.Where(x => x.Status == EBaseStatus.Active.ToString()).ToList();
        }

        var menuDto = _mapper.Map<MenuForKioskDto>(menu);

        menuDto.MenuProductMappings =
            await CheckAvailabilityForMenuProductsAsync(
                menuDto.MenuProductMappings.ToList(),
                kiosk.KioskDevices.ToList()
            );

        return new BaseResult<string, MenuForKioskDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Menu>(),
            Request = kiosk.MenuId,
            Response = menuDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateMenuDto, MenuDto>> CreateMenu(CreateMenuDto createMenuDto)
    {
        LogMessage(LogLevel.Information, "In CreateDevice", createMenuDto);

        var organization = await _unitOfWork.GetRepository<Organization>().SingleOrDefaultAsync(
            predicate: x => x.OrganizationId == createMenuDto.OrganizationId
        );

        if (organization is null)
        {
            return new BaseResult<CreateMenuDto, MenuDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Organization>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = createMenuDto
            };
        }

        var menu = _mapper.Map<Menu>(createMenuDto);

        await _unitOfWork.GetRepository<Menu>().InsertAsync(menu);
        await _unitOfWork.CommitAsync();

        var result = await _unitOfWork.CommitAsync();

        LogMessage(LogLevel.Information, "Insert Menu", result);

        var deviceDto = _mapper.Map<MenuDto>(menu);

        LogMessage(LogLevel.Information, "Out Create Menu", deviceDto);

        return new BaseResult<CreateMenuDto, MenuDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<Device>(),
            Request = createMenuDto,
            Response = deviceDto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<CloneMenuDto, MenuDto>> CloneMenu(CloneMenuDto cloneMenuDto)
    {
        var accountId = GetAccountIdFromJwt();

        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.AccountId == accountId
        );

        if (account is null)
        {
            return new BaseResult<CloneMenuDto, MenuDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Account>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = cloneMenuDto
            };
        }

        var existMenu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == cloneMenuDto.MenuId,
            include: x => x.Include(x => x.MenuProductMappings)
        );

        if (existMenu is null)
        {
            return new BaseResult<CloneMenuDto, MenuDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Menu>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = cloneMenuDto
            };
        }

        if (account.RoleName == nameof(ERoleName.Organization))
        {
            var organization = await _unitOfWork.GetRepository<Organization>().SingleOrDefaultAsync(
                predicate: x => x.OrganizationId == account.OrganizationId
            );

            if (organization is null)
            {
                return new BaseResult<CloneMenuDto, MenuDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Organization>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null,
                    Request = cloneMenuDto
                };
            }

            if (!existMenu.OrganizationId!.Equals(organization.OrganizationId))
            {
                return new BaseResult<CloneMenuDto, MenuDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<Menu>(),
                    StatusCode = StatusCodes.Status400BadRequest,
                    Response = null,
                    Request = cloneMenuDto
                };
            }
        }

        var newMenu = _mapper.Map<Menu>(existMenu);

        await _unitOfWork.GetRepository<Menu>().InsertAsync(newMenu);
        await _unitOfWork.CommitAsync();

        var newMenuDto = _mapper.Map<MenuDto>(newMenu);

        return new BaseResult<CloneMenuDto, MenuDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<Menu>(),
            StatusCode = StatusCodes.Status201Created,
            Response = newMenuDto,
            Request = cloneMenuDto
        };
    }

    public async Task<BaseResult<UpdateMenuDto, MenuDto>> UpdateMenu(string menuId, UpdateMenuDto updateMenuDto)
    {
        LogMessage(LogLevel.Information, "In UpdateMenu", updateMenuDto);

        var roles = GetAccountRolesFromJwt();

        if (roles[0].Equals(ERoleName.Organization.ToString()))
        {
            var referenceId = GetReferenceIdFromJwt();
            updateMenuDto.OrganizationId = referenceId ?? updateMenuDto.OrganizationId;
        }

        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId
        );

        if (menu is null)
        {
            return new BaseResult<UpdateMenuDto, MenuDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Menu>(),
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
            Message = MessageUtil.UpdateSuccess<Menu>(),
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
                Message = MessageUtil.NotFound<Menu>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = menuId
            };
        }

        var productInMenu = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId);

        if (productInMenu is not null)
        {
            return new BaseResult<string, MenuDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<Menu>(),
                StatusCode = StatusCodes.Status400BadRequest,
                Response = null,
                Request = menuId
            };
        }

        menu.Delete();

        _unitOfWork.GetRepository<Menu>().Update(menu);
        await _unitOfWork.CommitAsync();

        var menuDto = _mapper.Map<MenuDto>(menu);

        return new BaseResult<string, MenuDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<Menu>(),
            StatusCode = StatusCodes.Status202Accepted,
            Response = menuDto,
            Request = menuId
        };
    }

    public async Task<BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>> CreateMenuProductMapping(
        CreateMenuProductMappingDto createMenuProductMappingDto)
    {
        var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
            predicate: x => x.ProductId == createMenuProductMappingDto.ProductId
        );

        if (product is null)
        {
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Menu>(),
                Request = createMenuProductMappingDto,
                Response = null
            };
        }

        var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == createMenuProductMappingDto.MenuId
        );

        if (menu is null)
        {
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Menu>(),
                Request = createMenuProductMappingDto,
                Response = null
            };
        }

        // Check product suitable with menu which assign into kiosk

        var productChildren = await _unitOfWork.GetRepository<Product>().GetListAsync(
            predicate: x => x.ParentId == product.ProductId
        );


        // 1) Tập required 
        var requiredIds = productChildren.Select(c => c.ProductId).ToHashSet();

        if (requiredIds.IsNullOrEmpty())
        {
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.RequireChildEntity<Product>(),
                Request = createMenuProductMappingDto,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        // (tuỳ chọn) map Id -> Name để báo lỗi rõ ràng
        var nameById = new Dictionary<string, string> { };
        foreach (var c in productChildren) nameById[c.ProductId] = c.Name;

        // 2) Lấy danh sách KioskVersionId duy nhất đang được dùng bởi các kiosk gán menu này
        var kioskVersionIds = await _unitOfWork.GetRepository<Kiosk>().GetListAsync(
            predicate: x => x.MenuId == menu.MenuId && x.KioskVersionId != null,
            selector: x => x.KioskVersionId
        );

        kioskVersionIds = kioskVersionIds.Distinct().ToList();


        // 3) Load các KioskVersion (kèm mappings) theo danh sách Id ở trên
        var kioskVersions = await _unitOfWork.GetRepository<KioskVersion>().GetListAsync(
            predicate: v => kioskVersionIds.Contains(v.KioskVersionId),
            include: v =>
                v.Include(v => v.KioskVersionProductMappings).ThenInclude(x => x.Product).ThenInclude(x => x.Workflows)
        );

        // 4) Kiểm tra theo từng phiên bản
        var errors = new List<string>();
        var errorWorkflows = new List<string>();

        foreach (var ver in kioskVersions)
        {
            var supportedIds = new HashSet<string>(
                ver.KioskVersionProductMappings?.Select(m => m.ProductId) ?? []
            );

            var missingWorkflowIds = new HashSet<string>(
                ver.KioskVersionProductMappings?.Where(x => x.Product.Workflows.IsNullOrEmpty())
                    .Select(x => x.ProductId) ?? []);

            var missingWorkflow = requiredIds.Where(id => missingWorkflowIds.Contains(id))
                .Select(id => nameById.TryGetValue(id, out var n) ? n : id)
                .ToList();

            var supportsAnyChild = requiredIds.Overlaps(supportedIds);
            if (!supportsAnyChild)
            {
                var versionName = ver.VersionTitle ?? ver.KioskVersionId;
                var requiredNames = requiredIds.Select(id => nameById.TryGetValue(id, out var n) ? n : id);
                errors.Add(
                    $"Phiên bản '{versionName}' không hỗ trợ *bất kỳ* phiên bản con nào của sản phẩm gốc. " +
                    $"Cần ít nhất 1 trong: {string.Join(", ", requiredNames)}"
                );
            }
            
            if (missingWorkflow.Count > 0)
            {
                var versionName = ver.VersionTitle ?? ver.KioskVersionId;
                errorWorkflows.Add(
                    $"Phiên bản '{versionName}' thiếu quy trình cho món {string.Join(", ", missingWorkflow)}");
            }
        }

        if (errors.Count > 0)
        {
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.UnsupportedEntities<Product, Kiosk>(errors),
                Request = createMenuProductMappingDto,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        if (errorWorkflows.Count > 0)
        {
            return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.MissingWorkflows<Product, Kiosk>(errorWorkflows),
                Request = createMenuProductMappingDto,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }


        var total = await _unitOfWork.GetRepository<MenuProductMapping>().GetListAsync();

        var menuProductMapping = _mapper.Map<MenuProductMapping>(
            createMenuProductMappingDto,
            opts =>
            {
                opts.Items["DisplayOrder"] = total.Count + 1;
                opts.Items["SellingPrice"] = createMenuProductMappingDto.SellingPrice ?? product.Price;
            }
        );

        await _unitOfWork.GetRepository<MenuProductMapping>().InsertAsync(menuProductMapping);
        await _unitOfWork.CommitAsync();

        var menuProductMappingDto = _mapper.Map<MenuProductMappingDto>(menuProductMapping);
        return new BaseResult<CreateMenuProductMappingDto, MenuProductMappingDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.AddSuccess<Product>(),
            Request = createMenuProductMappingDto,
            Response = menuProductMappingDto
        };
    }

    public async Task<BaseResult<UpdateMenuProductMappingDto, MenuProductMappingDto>> UpdateMenuProductMapping(
        string menuId,
        string productId, UpdateMenuProductMappingDto updateMenuProductMappingDto)
    {
        var menuProductMapping = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId && x.ProductId == productId
        );

        if (menuProductMapping is null)
        {
            return new BaseResult<UpdateMenuProductMappingDto, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Product>(),
                Request = updateMenuProductMappingDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        menuProductMapping = _mapper.Map(updateMenuProductMappingDto, menuProductMapping);

        _unitOfWork.GetRepository<MenuProductMapping>().Update(menuProductMapping);
        await _unitOfWork.CommitAsync();

        var menuProductMappingDto = _mapper.Map<MenuProductMappingDto>(menuProductMapping);

        return new BaseResult<UpdateMenuProductMappingDto, MenuProductMappingDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<Product>(),
            Request = updateMenuProductMappingDto,
            Response = menuProductMappingDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<MenuProductMappingQueryDto, Paginate<MenuProductMappingDto>>> GetMenuProductMappings(
        MenuProductMappingQueryDto menuProductMappingQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetMenuProductMappings", menuProductMappingQueryDto);

        var predicate = _unitOfWork.GetRepository<MenuProductMapping>()
            .BuildSearchPredicate(menuProductMappingQueryDto.FilterQuery, menuProductMappingQueryDto.FilterBy);

        Expression<Func<MenuProductMapping, bool>> isDeletedFilter = x =>
            x.IsDeleted == false;
        predicate = ExpressionHelper.CombineExpressions<MenuProductMapping>(predicate, isDeletedFilter);


        if (menuProductMappingQueryDto.Status is not null)
        {
            Expression<Func<MenuProductMapping, bool>>
                statusFilter = x => x.Status == menuProductMappingQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions<MenuProductMapping>(predicate, statusFilter);
        }

        var orderBy = _unitOfWork.GetRepository<MenuProductMapping>()
            .BuildSortingQuery(menuProductMappingQueryDto.SortBy, menuProductMappingQueryDto.IsAsc);

        var menuProductMappings = await _unitOfWork.GetRepository<MenuProductMapping>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: menuProductMappingQueryDto.Page,
            size: menuProductMappingQueryDto.Size,
            include: x => x.Include(x => x.Product)
        );

        var menuProductMappingDtos = _mapper.Map<Paginate<MenuProductMappingDto>>(menuProductMappings);

        LogMessage(LogLevel.Information, "Out GetMenuProductMappings", menuProductMappingDtos);

        return new BaseResult<MenuProductMappingQueryDto, Paginate<MenuProductMappingDto>>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<MenuProductMapping>(),
            Request = menuProductMappingQueryDto,
            Response = menuProductMappingDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, MenuProductMappingDto>> GetMenuProductMapping(string menuId, string productId)
    {
        var menuProductMapping = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId && x.ProductId == productId,
            include: x => x.Include(x => x.Product).Include(x => x.Menu)
        );

        if (menuProductMapping is null)
        {
            return new BaseResult<string, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Product>(),
                Request = $"{menuId} {productId}",
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var menuProductMappingDto = _mapper.Map<MenuProductMappingDto>(menuProductMapping);

        return new BaseResult<string, MenuProductMappingDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<MenuProductMapping>(),
            Request = $"{menuId} {productId}",
            Response = menuProductMappingDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, MenuProductMappingDto>> RemoveMenuProductMapping(string menuId,
        string productId)
    {
        var menuProductMapping = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
            predicate: x => x.MenuId == menuId && x.ProductId == productId
        );

        if (menuProductMapping is null)
        {
            return new BaseResult<string, MenuProductMappingDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<MenuProductMapping>(),
                Request = $"{menuId} {productId}",
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        _unitOfWork.GetRepository<MenuProductMapping>().Delete(menuProductMapping);
        await _unitOfWork.CommitAsync();

        var menuProductMappingDto = _mapper.Map<MenuProductMappingDto>(menuProductMapping);

        return new BaseResult<string, MenuProductMappingDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<Product>(),
            Request = $"{menuId} {productId}",
            Response = menuProductMappingDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<ReorderMenuProductMappingDto, List<MenuProductMappingDto>>> ReorderMenuProductAsync(
        string menuId,
        ReorderMenuProductMappingDto reorderMenuProductMappingDto)
    {
        var menuProductMappings = await _unitOfWork.GetRepository<MenuProductMapping>().GetListAsync(
            predicate: x => x.MenuId == menuId,
            orderBy: q => q.OrderBy(x => x.DisplayOrder)
        );

        var list = menuProductMappings.ToList();

        var dragItem = list.FirstOrDefault(x => x.ProductId == reorderMenuProductMappingDto.DragProductId);
        var targetItem = list.FirstOrDefault(x => x.ProductId == reorderMenuProductMappingDto.TargetProductId);

        if (dragItem is null)
        {
            return new BaseResult<ReorderMenuProductMappingDto, List<MenuProductMappingDto>>
            {
                IsSuccess = false,
                Message = "Drag or target item not found",
                Request = reorderMenuProductMappingDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        if (targetItem is null)
        {
            return new BaseResult<ReorderMenuProductMappingDto, List<MenuProductMappingDto>>
            {
                IsSuccess = false,
                Message = "Drag or target item not found",
                Request = reorderMenuProductMappingDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        // Remove drag item temporarily
        list.Remove(dragItem);

        // Find index of target item
        var targetIndex = list.IndexOf(targetItem);
        var insertIndex = reorderMenuProductMappingDto.InsertAfter ? targetIndex + 1 : targetIndex;

        // Insert drag item to new position
        list.Insert(insertIndex, dragItem);

        // Reassign DisplayOrder
        for (int i = 0; i < list.Count; i++)
        {
            list[i].DisplayOrder = i + 1;
            _unitOfWork.GetRepository<MenuProductMapping>().Update(list[i]);
        }

        await _unitOfWork.CommitAsync();

        var menuProductMappingDtos = _mapper.Map<List<MenuProductMappingDto>>(menuProductMappings);

        return new BaseResult<ReorderMenuProductMappingDto, List<MenuProductMappingDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<Product>(),
            Request = reorderMenuProductMappingDto,
            Response = menuProductMappingDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }


    #region IngredientHelper

    /// <summary>
    /// CheckAvailabilityForMenuProductsAsync
    /// </summary>
    /// <param name="menuProducts"></param>
    /// <param name="kioskDevices"></param>
    public async Task<List<MenuProductMappingForKioskDto>> CheckAvailabilityForMenuProductsAsync(
        List<MenuProductMappingForKioskDto> menuProducts,
        List<KioskDeviceMapping> kioskDevices
    )
    {
        // B1: Gom nguyên liệu từ các thiết bị chính (IsPrimary = true)
        var primaryIngredientSources = new Dictionary<string, List<DeviceIngredientState>>();

        foreach (var mapping in kioskDevices)
        {
            var device = mapping.Device;
            if (device?.DeviceIngredientStates == null) continue;

            foreach (var state in device.DeviceIngredientStates)
            {
                if (!state.IsPrimary || state.IsWarning) continue;

                if (!primaryIngredientSources.TryGetValue(state.IngredientType, out var list))
                {
                    list = new List<DeviceIngredientState>();
                    primaryIngredientSources[state.IngredientType] = list;
                }

                list.Add(state);
            }
        }

        // B2: Kiểm tra từng món trong menu
        foreach (var menuProduct in menuProducts)
        {
            var product = menuProduct.Product;

            // Mặc định là có thể pha
            var isAvailable = true;

            if (product.ProductAttributes == null || !product.ProductAttributes.Any())
            {
                isAvailable = true;
            }
            else
            {
                foreach (var attr in product.ProductAttributes)
                {
                    var requiredAmount = attr.DefaultAmount;
                    var ingredientType = attr.IngredientType;

                    if (!primaryIngredientSources.TryGetValue(ingredientType, out var sources) || sources.Count == 0)
                    {
                        isAvailable = false;
                        break;
                    }

                    // Chỉ cần 1 thiết bị đủ lượng
                    var hasDeviceEnough = sources.Any(s => s.CurrentCapacity >= requiredAmount);
                    if (!hasDeviceEnough)
                    {
                        isAvailable = false;
                        break;
                    }
                }
            }

            // Cập nhật kết quả
            menuProduct.IsAvailable = isAvailable;
        }

        return menuProducts;
    }

    #endregion
}