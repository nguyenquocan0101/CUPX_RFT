using AutoMapper;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.DeviceType;
using Services.Interfaces;
using AutomaticBrewingCoffee.Domain.Models;
using System.Linq.Expressions;
using Services.Utils;

namespace Services.Implements;

public class DeviceTypeService : BaseService<DeviceTypeService>, IDeviceTypeService
{
    public DeviceTypeService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
    {
    }

    public async Task<BaseResult<DeviceTypeQueryDto, Paginate<DeviceTypeDto>>> GetDeviceTypes(
        DeviceTypeQueryDto deviceTypeQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetDeviceTypes", deviceTypeQueryDto);

        var predicate = _unitOfWork.GetRepository<DeviceType>()
            .BuildSearchPredicate(deviceTypeQueryDto.FilterQuery, deviceTypeQueryDto.FilterBy);

        Expression<Func<DeviceType, bool>> isDeletedFilter = x => x.IsDeleted == false;
        predicate = ExpressionHelper.CombineExpressions<DeviceType>(predicate, isDeletedFilter);

        if (deviceTypeQueryDto.StartDate is not null && deviceTypeQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<DeviceType>().BuildDateRangePredicate(
                deviceTypeQueryDto.StartDate,
                deviceTypeQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        if (!string.IsNullOrEmpty(deviceTypeQueryDto.Status))
        {
            Expression<Func<DeviceType, bool>> isStatus = x => x.Status == deviceTypeQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions<DeviceType>(predicate, isStatus);
        }

        var orderBy = _unitOfWork.GetRepository<DeviceType>()
            .BuildSortingQuery(deviceTypeQueryDto.SortBy, deviceTypeQueryDto.IsAsc);

        var deviceTypes = await _unitOfWork.GetRepository<DeviceType>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: deviceTypeQueryDto.Page,
            size: deviceTypeQueryDto.Size
        );

        var deviceTypeDto = _mapper.Map<Paginate<DeviceTypeDto>>(deviceTypes);

        LogMessage(LogLevel.Information, "Out GetDeviceTypes", deviceTypeDto);

        return new BaseResult<DeviceTypeQueryDto, Paginate<DeviceTypeDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<DeviceType>(),
            Request = deviceTypeQueryDto,
            Response = deviceTypeDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, DeviceTypeDto>> GetDeviceType(string deviceTypeId)
    {
        LogMessage(LogLevel.Information, "In GetDeviceType", deviceTypeId);

        var deviceType = await _unitOfWork.GetRepository<DeviceType>()
            .SingleOrDefaultAsync(predicate: x => x.DeviceTypeId == deviceTypeId);

        if (deviceType is null)
        {
            return new BaseResult<string, DeviceTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<DeviceType>(),
                Request = deviceTypeId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var deviceTypeDto = _mapper.Map<DeviceTypeDto>(deviceType);

        LogMessage(LogLevel.Information, "Out GetDeviceType", deviceTypeDto);

        return new BaseResult<string, DeviceTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<DeviceType>(),
            Request = deviceTypeId,
            Response = deviceTypeDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateDeviceTypeDto, DeviceTypeDto>> CreateDeviceType(
        CreateDeviceTypeDto createDeviceTypeDto)
    {
        LogMessage(LogLevel.Information, "In CreateDeviceType", createDeviceTypeDto);

        var newDeviceType = _mapper.Map<DeviceType>(createDeviceTypeDto);
        await _unitOfWork.GetRepository<DeviceType>().InsertAsync(newDeviceType);
        var result = await _unitOfWork.CommitAsync();

        LogMessage(LogLevel.Information, "Insert DeviceType", result);

        var deviceTypeDto = _mapper.Map<DeviceTypeDto>(newDeviceType);

        LogMessage(LogLevel.Information, "Out CreateDeviceType", deviceTypeDto);

        return new BaseResult<CreateDeviceTypeDto, DeviceTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<DeviceType>(),
            Request = createDeviceTypeDto,
            Response = deviceTypeDto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateDeviceTypeDto, DeviceTypeDto>> UpdateDeviceType(string deviceTypeId,
        UpdateDeviceTypeDto updateDeviceTypeDto)
    {
        var deviceType = await _unitOfWork.GetRepository<DeviceType>()
            .SingleOrDefaultAsync(predicate: x => x.DeviceTypeId == deviceTypeId);

        if (deviceType is null)
        {
            return new BaseResult<UpdateDeviceTypeDto, DeviceTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<DeviceType>(),
                Request = updateDeviceTypeDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        deviceType = _mapper.Map(updateDeviceTypeDto, deviceType);

        _unitOfWork.GetRepository<DeviceType>().Update(deviceType);
        await _unitOfWork.CommitAsync();

        var deviceTypeDto = _mapper.Map<DeviceTypeDto>(deviceType);

        return new BaseResult<UpdateDeviceTypeDto, DeviceTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<DeviceType>(),
            Request = updateDeviceTypeDto,
            Response = deviceTypeDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<string, DeviceTypeDto>> RemoveDeviceType(string deviceTypeId)
    {
        LogMessage(LogLevel.Information, "In RemoveDeviceType", deviceTypeId);

        var deviceType = await _unitOfWork.GetRepository<DeviceType>()
            .SingleOrDefaultAsync(predicate: x => x.DeviceTypeId == deviceTypeId);

        if (deviceType is null)
        {
            return new BaseResult<string, DeviceTypeDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<DeviceType>(),
                Request = deviceTypeId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var deviceModelWithDeviceType = await _unitOfWork.GetRepository<DeviceModel>().SingleOrDefaultAsync(
            predicate: x => x.DeviceTypeId == deviceTypeId
        );

        if (deviceModelWithDeviceType is not null)
        {
            return new BaseResult<string, DeviceTypeDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<DeviceModel>(),
                Request = deviceTypeId,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        deviceType.Delete();

        _unitOfWork.GetRepository<DeviceType>().Update(deviceType);
        await _unitOfWork.CommitAsync();

        var deviceTypeDto = _mapper.Map<DeviceTypeDto>(deviceType);

        LogMessage(LogLevel.Information, "Out RemoveDeviceType", deviceTypeDto);

        return new BaseResult<string, DeviceTypeDto>
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<DeviceType>(),
            Request = deviceTypeId,
            Response = deviceTypeDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }
}