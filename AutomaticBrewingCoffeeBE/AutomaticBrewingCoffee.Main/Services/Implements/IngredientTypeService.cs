using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Interfaces;
using Services.Utils;
using Services.Dtos.IngredientType;

namespace Services.Implements;

public class IngredientTypeService : BaseService<IngredientTypeService>, IIngredientTypeService
{
    public IngredientTypeService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor
    ) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
    {
    }

    public async Task<BaseResult<IngredientTypeQueryDto, Paginate<IngredientTypeDto>>> GetIngredientTypes(
        IngredientTypeQueryDto queryDto)
    {
        LogMessage(LogLevel.Information, "In GetIngredientTypes", queryDto);

        var predicate = _unitOfWork.GetRepository<IngredientType>()
            .BuildSearchPredicate(queryDto.FilterQuery, queryDto.FilterBy);

        predicate = ExpressionHelper.CombineExpressions(predicate, x => !x.IsDeleted);

        if (queryDto.StartDate is not null && queryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<IngredientType>().BuildDateRangePredicate(
                queryDto.StartDate,
                queryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        if (!string.IsNullOrEmpty(queryDto.Status))
        {
            predicate = ExpressionHelper.CombineExpressions(predicate, x => x.Status == queryDto.Status);
        }

        var orderBy = _unitOfWork.GetRepository<IngredientType>()
            .BuildSortingQuery(queryDto.SortBy, queryDto.IsAsc);

        var result = await _unitOfWork.GetRepository<IngredientType>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: queryDto.Page,
            size: queryDto.Size
        );

        var dto = _mapper.Map<Paginate<IngredientTypeDto>>(result);

        LogMessage(LogLevel.Information, "Out GetIngredientTypes", dto);

        return new BaseResult<IngredientTypeQueryDto, Paginate<IngredientTypeDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<IngredientType>(),
            Request = queryDto,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, IngredientTypeDto>> GetIngredientType(string id)
    {
        LogMessage(LogLevel.Information, "In GetIngredientType", id);

        var entity = await _unitOfWork.GetRepository<IngredientType>()
            .SingleOrDefaultAsync(predicate: x => x.IngredientTypeId == id && !x.IsDeleted);

        if (entity == null)
        {
            return new BaseResult<string, IngredientTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<IngredientType>(),
                Request = id,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var dto = _mapper.Map<IngredientTypeDto>(entity);

        LogMessage(LogLevel.Information, "Out GetIngredientType", dto);

        return new BaseResult<string, IngredientTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<IngredientType>(),
            Request = id,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateIngredientTypeDto, IngredientTypeDto>> CreateIngredientType(
        CreateIngredientTypeDto createDto)
    {
        LogMessage(LogLevel.Information, "In CreateIngredientType", createDto);

        var entity = _mapper.Map<IngredientType>(createDto);
        await _unitOfWork.GetRepository<IngredientType>().InsertAsync(entity);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<IngredientTypeDto>(entity);

        return new BaseResult<CreateIngredientTypeDto, IngredientTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<IngredientType>(),
            Request = createDto,
            Response = dto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateIngredientTypeDto, IngredientTypeDto>> UpdateIngredientType(string id,
        UpdateIngredientTypeDto updateDto)
    {
        var entity = await _unitOfWork.GetRepository<IngredientType>()
            .SingleOrDefaultAsync(predicate: x => x.IngredientTypeId == id && !x.IsDeleted);

        if (entity == null)
        {
            return new BaseResult<UpdateIngredientTypeDto, IngredientTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<IngredientType>(),
                Request = updateDto,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        entity = _mapper.Map(updateDto, entity);
        _unitOfWork.GetRepository<IngredientType>().Update(entity);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<IngredientTypeDto>(entity);

        return new BaseResult<UpdateIngredientTypeDto, IngredientTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<IngredientType>(),
            Request = updateDto,
            Response = dto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<string, IngredientTypeDto>> RemoveIngredientType(string id)
    {
        LogMessage(LogLevel.Information, "In RemoveIngredientType", id);

        var entity = await _unitOfWork.GetRepository<IngredientType>()
            .SingleOrDefaultAsync(predicate: x => x.IngredientTypeId == id && !x.IsDeleted);

        if (entity == null)
        {
            return new BaseResult<string, IngredientTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<IngredientType>(),
                Request = id,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var deviceIngredient = await _unitOfWork.GetRepository<DeviceIngredient>().SingleOrDefaultAsync(
            predicate: x => x.IngredientType == entity.Name
        );

        if (deviceIngredient is not null)
        {
            return new BaseResult<string, IngredientTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<IngredientType>(),
                Request = id,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        entity.Delete();
        _unitOfWork.GetRepository<IngredientType>().Update(entity);
        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<IngredientTypeDto>(entity);

        return new BaseResult<string, IngredientTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<IngredientType>(),
            Request = id,
            Response = dto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }
}