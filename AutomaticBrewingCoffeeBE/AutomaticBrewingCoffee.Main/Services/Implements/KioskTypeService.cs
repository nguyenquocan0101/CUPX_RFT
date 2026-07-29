using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.KioskType;
using System.Linq.Expressions;
using Services.Interfaces;
using Services.Utils;

namespace Services.Implements;

public class KioskTypeService : BaseService<KioskTypeService>, IKioskTypeService
{
    public KioskTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
    {
    }

    public async Task<BaseResult<KioskTypeQueryDto, Paginate<KioskTypeDto>>> GetKioskTypes(
        KioskTypeQueryDto kioskTypeQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetKioskTypes", kioskTypeQueryDto);

        var predicate = _unitOfWork.GetRepository<KioskType>()
            .BuildSearchPredicate(kioskTypeQueryDto.FilterQuery, kioskTypeQueryDto.FilterBy);

        Expression<Func<KioskType, bool>> isDeletedFilter = x => !x.IsDeleted;
        predicate = ExpressionHelper.CombineExpressions(predicate, isDeletedFilter);

        if (kioskTypeQueryDto.StartDate is not null && kioskTypeQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<KioskType>().BuildDateRangePredicate(
                kioskTypeQueryDto.StartDate,
                kioskTypeQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        if (!string.IsNullOrEmpty(kioskTypeQueryDto.Status))
        {
            Expression<Func<KioskType, bool>> isStatus = x => x.Status == kioskTypeQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions(predicate, isStatus);
        }

        var orderBy = _unitOfWork.GetRepository<KioskType>()
            .BuildSortingQuery(kioskTypeQueryDto.SortBy, kioskTypeQueryDto.IsAsc);

        var kioskTypes = await _unitOfWork.GetRepository<KioskType>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: kioskTypeQueryDto.Page,
            size: kioskTypeQueryDto.Size
        );

        var dto = _mapper.Map<Paginate<KioskTypeDto>>(kioskTypes);

        LogMessage(LogLevel.Information, "Out GetKioskTypes", dto);

        return new BaseResult<KioskTypeQueryDto, Paginate<KioskTypeDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.NotFound<KioskType>(),
            Request = kioskTypeQueryDto,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, KioskTypeDto>> GetKioskType(string kioskTypeId)
    {
        LogMessage(LogLevel.Information, "In GetKioskType", kioskTypeId);

        var kioskType = await _unitOfWork.GetRepository<KioskType>()
            .SingleOrDefaultAsync(predicate: x => x.KioskTypeId == kioskTypeId);

        if (kioskType == null)
        {
            return new BaseResult<string, KioskTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskType>(),
                Request = kioskTypeId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var dto = _mapper.Map<KioskTypeDto>(kioskType);

        LogMessage(LogLevel.Information, "Out GetKioskType", dto);

        return new BaseResult<string, KioskTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<KioskType>(),
            Request = kioskTypeId,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateKioskTypeDto, KioskTypeDto>> CreateKioskType(CreateKioskTypeDto createDto)
    {
        LogMessage(LogLevel.Information, "In CreateKioskType", createDto);

        var entity = _mapper.Map<KioskType>(createDto);
        await _unitOfWork.GetRepository<KioskType>().InsertAsync(entity);
        var result = await _unitOfWork.CommitAsync();

        LogMessage(LogLevel.Information, "Insert KioskType", result);

        var dto = _mapper.Map<KioskTypeDto>(entity);

        LogMessage(LogLevel.Information, "Out CreateKioskType", dto);

        return new BaseResult<CreateKioskTypeDto, KioskTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<KioskType>(),
            Request = createDto,
            Response = dto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateKioskTypeDto, KioskTypeDto>> UpdateKioskType(string kioskTypeId,
        UpdateKioskTypeDto updateDto)
    {
        var entity = await _unitOfWork.GetRepository<KioskType>()
            .SingleOrDefaultAsync(
                predicate: x => x.KioskTypeId == kioskTypeId);

        if (entity == null)
        {
            return new BaseResult<UpdateKioskTypeDto, KioskTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskType>(),
                Request = updateDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        entity = _mapper.Map(updateDto, entity);
        _unitOfWork.GetRepository<KioskType>().Update(entity);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<KioskTypeDto>(entity);

        return new BaseResult<UpdateKioskTypeDto, KioskTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<KioskType>(),
            Request = updateDto,
            Response = dto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<string, KioskTypeDto>> RemoveKioskType(string kioskTypeId)
    {
        LogMessage(LogLevel.Information, "In RemoveKioskType", kioskTypeId);

        var entity = await _unitOfWork.GetRepository<KioskType>().SingleOrDefaultAsync(
            predicate: x => x.KioskTypeId == kioskTypeId);

        if (entity == null)
        {
            return new BaseResult<string, KioskTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<KioskType>(),
                Request = kioskTypeId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var kioskVersionOf = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
            predicate: x => x.KioskTypeId == kioskTypeId
        );

        if (kioskVersionOf is not null)
        {
            return new BaseResult<string, KioskTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<KioskType>(),
                Request = kioskTypeId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        entity.Delete();
        _unitOfWork.GetRepository<KioskType>().Update(entity);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<KioskTypeDto>(entity);

        LogMessage(LogLevel.Information, "Out RemoveKioskType", dto);

        return new BaseResult<string, KioskTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<KioskType>(),
            Request = kioskTypeId,
            Response = dto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }
}