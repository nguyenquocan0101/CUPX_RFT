using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.KioskVersion;
using Services.Interfaces;
using System.Linq.Expressions;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Services.Dtos.Device;
using Services.Dtos.KioskVersionDeviceModel;
using Services.Dtos.KioskVersionProduct;
using Services.Utils;

namespace Services.Implements;

public class KioskVersionService : BaseService<KioskVersionService>, IKioskVersionService
{
    public KioskVersionService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
    {
    }

    public async Task<BaseResult<KioskVersionQueryDto, Paginate<KioskVersionDto>>> GetKioskVersions(
        KioskVersionQueryDto kioskVersionQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetKioskVersions", kioskVersionQueryDto);

        var predicate = _unitOfWork.GetRepository<KioskVersion>()
            .BuildSearchPredicate(kioskVersionQueryDto.FilterQuery, kioskVersionQueryDto.FilterBy);

        Expression<Func<KioskVersion, bool>> isDeletedFilter = x => !x.IsDeleted;
        predicate = ExpressionHelper.CombineExpressions(predicate, isDeletedFilter);

        if (kioskVersionQueryDto.StartDate is not null && kioskVersionQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<KioskVersion>().BuildDateRangePredicate(
                kioskVersionQueryDto.StartDate,
                kioskVersionQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        if (!string.IsNullOrEmpty(kioskVersionQueryDto.Status))
        {
            Expression<Func<KioskVersion, bool>> isStatus = x => x.Status == kioskVersionQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions(predicate, isStatus);
        }

        var orderBy = _unitOfWork.GetRepository<KioskVersion>()
            .BuildSortingQuery(kioskVersionQueryDto.SortBy, kioskVersionQueryDto.IsAsc);

        var kioskVersions = await _unitOfWork.GetRepository<KioskVersion>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: kioskVersionQueryDto.Page,
            size: kioskVersionQueryDto.Size,
            include: x => x.Include(x => x.KioskVersionDeviceModelMappings)
                .ThenInclude(x => x.DeviceModel)
                .ThenInclude(x => x.DeviceType)
                .Include(x => x.KioskVersionProductMappings)
                .ThenInclude(x => x.Product)
                .Include(x => x.KioskType)
        );

        var dto = _mapper.Map<Paginate<KioskVersionDto>>(kioskVersions);

        LogMessage(LogLevel.Information, "Out GetKioskVersions", dto);

        return new BaseResult<KioskVersionQueryDto, Paginate<KioskVersionDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<KioskVersion>(),
            Request = kioskVersionQueryDto,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, KioskVersionDto>> GetKioskVersion(string kioskVersionId)
    {
        LogMessage(LogLevel.Information, "In GetKioskVersion", kioskVersionId);

        var entity = await _unitOfWork.GetRepository<KioskVersion>()
            .SingleOrDefaultAsync(
                predicate: x => x.KioskVersionId == kioskVersionId,
                include: x => x.Include(x => x.KioskVersionDeviceModelMappings)
                    .ThenInclude(x => x.DeviceModel)
                    .ThenInclude(x => x.DeviceType)
                    .Include(x => x.KioskType)
            );

        if (entity == null)
        {
            return new BaseResult<string, KioskVersionDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskVersion>(),
                Request = kioskVersionId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var dto = _mapper.Map<KioskVersionDto>(entity);

        LogMessage(LogLevel.Information, "Out GetKioskVersion", dto);

        return new BaseResult<string, KioskVersionDto>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<KioskVersion>(),
            Request = kioskVersionId,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateKioskVersionDto, KioskVersionDto>> CreateKioskVersion(
        CreateKioskVersionDto createDto)
    {
        LogMessage(LogLevel.Information, "In CreateKioskVersion", createDto);

        var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
            predicate: x => x.VersionNumber.Trim() == createDto.VersionNumber.Trim()
        );

        if (kioskVersion is not null)
        {
            return new BaseResult<CreateKioskVersionDto, KioskVersionDto>
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyExists<KioskVersion>(kioskVersion.VersionNumber),
                Request = createDto,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var entity = _mapper.Map<KioskVersion>(createDto);
        await _unitOfWork.GetRepository<KioskVersion>().InsertAsync(entity);
        var result = await _unitOfWork.CommitAsync();

        LogMessage(LogLevel.Information, "Insert KioskVersion", result);

        var dto = _mapper.Map<KioskVersionDto>(entity);

        LogMessage(LogLevel.Information, "Out CreateKioskVersion", dto);

        return new BaseResult<CreateKioskVersionDto, KioskVersionDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<KioskVersion>(),
            Request = createDto,
            Response = dto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateKioskVersionDto, KioskVersionDto>> UpdateKioskVersion(
        string kioskVersionId, UpdateKioskVersionDto updateDto)
    {
        var entity = await _unitOfWork.GetRepository<KioskVersion>()
            .SingleOrDefaultAsync(predicate: x => x.KioskVersionId == kioskVersionId);

        if (entity == null)
        {
            return new BaseResult<UpdateKioskVersionDto, KioskVersionDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskVersion>(),
                Request = updateDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        if (entity.VersionNumber != updateDto.VersionNumber)
        {
            var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
                predicate: x => x.VersionNumber.Trim() == updateDto.VersionNumber.Trim()
            );

            if (kioskVersion is not null)
            {
                return new BaseResult<UpdateKioskVersionDto, KioskVersionDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.AlreadyExists<KioskVersion>(kioskVersion.VersionNumber),
                    Request = updateDto,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        entity = _mapper.Map(updateDto, entity);
        _unitOfWork.GetRepository<KioskVersion>().Update(entity);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<KioskVersionDto>(entity);

        return new BaseResult<UpdateKioskVersionDto, KioskVersionDto>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<KioskVersion>(),
            Request = updateDto,
            Response = dto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<string, KioskVersionDto>> RemoveKioskVersion(string kioskVersionId)
    {
        LogMessage(LogLevel.Information, "In RemoveKioskVersion", kioskVersionId);

        var entity = await _unitOfWork.GetRepository<KioskVersion>()
            .SingleOrDefaultAsync(predicate: x => x.KioskVersionId == kioskVersionId);

        if (entity == null)
        {
            return new BaseResult<string, KioskVersionDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskVersion>(),
                Request = kioskVersionId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskVersionId == kioskVersionId
        );

        if (kiosk is not null)
        {
            return new BaseResult<string, KioskVersionDto>
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<KioskVersion>(),
                Request = kioskVersionId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        entity.Delete();
        _unitOfWork.GetRepository<KioskVersion>().Update(entity);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<KioskVersionDto>(entity);

        LogMessage(LogLevel.Information, "Out RemoveKioskVersion", dto);

        return new BaseResult<string, KioskVersionDto>
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<KioskVersion>(),
            Request = kioskVersionId,
            Response = dto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelDto>> AddKioskVersionDeviceModel(
        AddKioskVersionDeviceModelDto addKioskVersionDeviceModelDto)
    {
        var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
            predicate: x => x.KioskVersionId == addKioskVersionDeviceModelDto.KioskVersionId);

        if (kioskVersion is null)
        {
            return new BaseResult<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskVersion>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = addKioskVersionDeviceModelDto
            };
        }

        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskVersionId == kioskVersion.KioskVersionId
        );

        if (kiosk is not null)
        {
            return new BaseResult<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<KioskVersion>(),
                Response = null,
                Request = addKioskVersionDeviceModelDto,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        var deviceModel = await _unitOfWork.GetRepository<DeviceModel>().SingleOrDefaultAsync(
            predicate: x => x.DeviceModelId == addKioskVersionDeviceModelDto.DeviceModelId);

        if (deviceModel is null)
        {
            return new BaseResult<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<DeviceModel>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = addKioskVersionDeviceModelDto
            };
        }

        var kioskVersionDeviceModelMapping = await _unitOfWork.GetRepository<KioskVersionDeviceModelMapping>()
            .SingleOrDefaultAsync(
                predicate: x => x.KioskVersionId == addKioskVersionDeviceModelDto.KioskVersionId
                                && x.DeviceModelId == addKioskVersionDeviceModelDto.DeviceModelId
            );

        if (kioskVersionDeviceModelMapping is null)
        {
            kioskVersionDeviceModelMapping = _mapper.Map<KioskVersionDeviceModelMapping>(addKioskVersionDeviceModelDto);
            await _unitOfWork.GetRepository<KioskVersionDeviceModelMapping>()
                .InsertAsync(kioskVersionDeviceModelMapping);
        }
        else
        {
            kioskVersionDeviceModelMapping.Quantity += addKioskVersionDeviceModelDto.Quantity;
            kioskVersionDeviceModelMapping.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.GetRepository<KioskVersionDeviceModelMapping>()
                .Update(kioskVersionDeviceModelMapping);
        }

        await _unitOfWork.CommitAsync();

        var kioskVersionDeviceModelDto = _mapper.Map<KioskVersionDeviceModelDto>(kioskVersionDeviceModelMapping);

        return new BaseResult<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.AddSuccess<DeviceModel>(),
            StatusCode = StatusCodes.Status201Created,
            Response = kioskVersionDeviceModelDto,
            Request = addKioskVersionDeviceModelDto
        };
    }

    public async Task<BaseResult<string, Paginate<KioskVersionDeviceModelDto>>> GetKioskVersionDeviceModels(
        string kioskVersionId,
        KioskVersionDeviceModelQueryDto kioskVersionDeviceModelQueryDto
    )
    {
        LogMessage(LogLevel.Information, "In GetKioskVersions", kioskVersionDeviceModelQueryDto);

        var predicate = _unitOfWork.GetRepository<KioskVersionDeviceModelMapping>()
            .BuildSearchPredicate(kioskVersionDeviceModelQueryDto.FilterQuery,
                kioskVersionDeviceModelQueryDto.FilterBy);

        var orderBy = _unitOfWork.GetRepository<KioskVersionDeviceModelMapping>()
            .BuildSortingQuery(kioskVersionDeviceModelQueryDto.SortBy, kioskVersionDeviceModelQueryDto.IsAsc);

        var kioskVersions = await _unitOfWork.GetRepository<KioskVersionDeviceModelMapping>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: kioskVersionDeviceModelQueryDto.Page,
            size: kioskVersionDeviceModelQueryDto.Size
        );

        var dto = _mapper.Map<Paginate<KioskVersionDeviceModelDto>>(kioskVersions);

        LogMessage(LogLevel.Information, "Out GetKioskVersions", dto);

        return new BaseResult<string, Paginate<KioskVersionDeviceModelDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<DeviceModel>(),
            Request = kioskVersionId,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>> AddKioskVersionProduct(
        AddKioskVersionProductDto addKioskVersionProductDto)
    {
        var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
            predicate: x => x.KioskVersionId == addKioskVersionProductDto.KioskVersionId);

        if (kioskVersion is null)
        {
            return new BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskVersion>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = addKioskVersionProductDto
            };
        }

        var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
            predicate: x => x.ProductId == addKioskVersionProductDto.ProductId);

        if (product is null)
        {
            return new BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Product>(),
                StatusCode = StatusCodes.Status404NotFound,
                Response = null,
                Request = addKioskVersionProductDto
            };
        }

        var kioskVersionProductMapping = await _unitOfWork.GetRepository<KioskVersionProductMapping>()
            .SingleOrDefaultAsync(
                predicate: x => x.KioskVersionId == addKioskVersionProductDto.KioskVersionId
                                && x.ProductId == addKioskVersionProductDto.ProductId
            );

        if (kioskVersionProductMapping is not null)
        {
            return new BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyExists<Product>(),
                Request = addKioskVersionProductDto,
                StatusCode = StatusCodes.Status400BadRequest,
                Response = null
            };
        }

        var productWithSameParent = await _unitOfWork.GetRepository<KioskVersionProductMapping>()
            .SingleOrDefaultAsync(
                predicate: x => x.KioskVersionId == addKioskVersionProductDto.KioskVersionId
                                && x.Product.ParentId == product.ParentId,
                include: x => x.Include(x => x.Product)
            );

        if (productWithSameParent is not null)
        {
            return new BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.Invalid<Product>(),
                Request = addKioskVersionProductDto,
                StatusCode = StatusCodes.Status400BadRequest,
                Response = null
            };
        }

        kioskVersionProductMapping = _mapper.Map<KioskVersionProductMapping>(addKioskVersionProductDto);

        await _unitOfWork.GetRepository<KioskVersionProductMapping>()
            .InsertAsync(kioskVersionProductMapping);


        await _unitOfWork.CommitAsync();

        var kioskVersionProductDto = _mapper.Map<KioskVersionProductDto>(kioskVersionProductMapping);

        return new BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>
        {
            IsSuccess = true,
            Message = MessageUtil.AddSuccess<DeviceModel>(),
            StatusCode = StatusCodes.Status201Created,
            Response = kioskVersionProductDto,
            Request = addKioskVersionProductDto
        };
    }

    public async Task<BaseResult<string, Paginate<KioskVersionProductDto>>> GetKioskVersionProduct(
        string kioskVersionId, KioskVersionProductQueryDto kioskVersionProductQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetKioskVersionProduct", kioskVersionProductQueryDto);

        var predicate = _unitOfWork.GetRepository<KioskVersionProductMapping>()
            .BuildSearchPredicate(kioskVersionProductQueryDto.FilterQuery,
                kioskVersionProductQueryDto.FilterBy);

        Expression<Func<KioskVersionProductMapping, bool>> matchId = x => x.KioskVersionId == kioskVersionId;
        predicate = ExpressionHelper.CombineExpressions(predicate, matchId);

        var orderBy = _unitOfWork.GetRepository<KioskVersionProductMapping>()
            .BuildSortingQuery(kioskVersionProductQueryDto.SortBy, kioskVersionProductQueryDto.IsAsc);

        var kioskVersions = await _unitOfWork.GetRepository<KioskVersionProductMapping>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: kioskVersionProductQueryDto.Page,
            size: kioskVersionProductQueryDto.Size,
            include: x => x.Include(x => x.Product),
            ignorePaging: true
        );

        var dto = _mapper.Map<Paginate<KioskVersionProductDto>>(kioskVersions);

        LogMessage(LogLevel.Information, "Out GetKioskVersionProduct", dto);

        return new BaseResult<string, Paginate<KioskVersionProductDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Product>(),
            Request = kioskVersionId,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, Paginate<DeviceDto>>> GetValidDevices(string kioskVersionId,
        DeviceQueryDto deviceQueryDto)
    {
        var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
            predicate: x => x.KioskVersionId == kioskVersionId,
            include: x => x.Include(x => x.KioskVersionDeviceModelMappings)
        );

        if (kioskVersion is null)
        {
            return new BaseResult<string, Paginate<DeviceDto>>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskVersion>(),
                Request = kioskVersionId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var predicate = _unitOfWork.GetRepository<Device>()
            .BuildSearchPredicate(deviceQueryDto.FilterQuery, deviceQueryDto.FilterBy);

        var deviceModelIds = kioskVersion.KioskVersionDeviceModelMappings.Select(x
            => x.DeviceModelId);

        Expression<Func<Device, bool>> validDeviceCheck = x => deviceModelIds.Contains(x.DeviceModelId);

        predicate = ExpressionHelper.CombineExpressions(predicate, validDeviceCheck);

        Expression<Func<Device, bool>> isStock = x => x.Status.Equals(EDeviceStatus.Stock.ToString());

        predicate = ExpressionHelper.CombineExpressions(predicate, isStock);

        var orderBy = _unitOfWork.GetRepository<Device>()
            .BuildSortingQuery(deviceQueryDto.SortBy, deviceQueryDto.IsAsc);

        var devices = await _unitOfWork.GetRepository<Device>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: deviceQueryDto.Page,
            size: deviceQueryDto.Size,
            include: x => x.Include(x => x.DeviceModel).ThenInclude(x => x.DeviceType)
        );

        var deviceDtos = _mapper.Map<Paginate<DeviceDto>>(devices);

        return new BaseResult<string, Paginate<DeviceDto>>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Device>(),
            Request = kioskVersionId,
            Response = deviceDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }
}