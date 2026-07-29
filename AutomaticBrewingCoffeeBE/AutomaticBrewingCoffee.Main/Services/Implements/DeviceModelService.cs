using AutoMapper;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.DeviceModel;
using Services.Interfaces;
using AutomaticBrewingCoffee.Domain.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Services.Utils;

namespace Services.Implements;

public class DeviceModelService : BaseService<DeviceModelService>, IDeviceModelService
{
    public DeviceModelService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
    {
    }

    public async Task<BaseResult<DeviceModelQueryDto, Paginate<DeviceModelDto>>> GetDeviceModels(
        DeviceModelQueryDto deviceModelQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetDeviceModels", deviceModelQueryDto);

        var predicate = _unitOfWork.GetRepository<DeviceModel>()
            .BuildSearchPredicate(deviceModelQueryDto.FilterQuery, deviceModelQueryDto.FilterBy);

        Expression<Func<DeviceModel, bool>> isDeletedFilter = x => x.IsDeleted == false;
        predicate = ExpressionHelper.CombineExpressions<DeviceModel>(predicate, isDeletedFilter);

        if (deviceModelQueryDto.StartDate is not null && deviceModelQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<DeviceModel>().BuildDateRangePredicate(
                deviceModelQueryDto.StartDate,
                deviceModelQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions<DeviceModel>(predicate, dateRangePredicate);
        }

        if (!string.IsNullOrEmpty(deviceModelQueryDto.Status))
        {
            Expression<Func<DeviceModel, bool>> isStatusFilter = x => x.Status == deviceModelQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions<DeviceModel>(predicate, isStatusFilter);
        }

        if (deviceModelQueryDto.KioskVersionId is not null)
        {
            var kioskVersionDeviceModels = await _unitOfWork.GetRepository<KioskVersionDeviceModelMapping>()
                .GetListAsync(
                    predicate: x => x.KioskVersionId == deviceModelQueryDto.KioskVersionId
                );

            var existDeviceIds = kioskVersionDeviceModels.Select(x => x.DeviceModelId);

            Expression<Func<DeviceModel, bool>> isStatus = x => existDeviceIds.Contains(x.DeviceModelId);
            predicate = ExpressionHelper.CombineExpressions<DeviceModel>(predicate, isStatus);
        }

        var orderBy = _unitOfWork.GetRepository<DeviceModel>()
            .BuildSortingQuery(deviceModelQueryDto.SortBy, deviceModelQueryDto.IsAsc);

        var deviceModels = await _unitOfWork.GetRepository<DeviceModel>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: deviceModelQueryDto.Page,
            size: deviceModelQueryDto.Size,
            include: x => x.Include(x => x.DeviceType)
                .Include(x => x.DeviceFunctions)
                .ThenInclude(x => x.FunctionParameters)
                .Include(x => x.DeviceIngredients)
        );

        var deviceModelDto = _mapper.Map<Paginate<DeviceModelDto>>(deviceModels);

        LogMessage(LogLevel.Information, "Out GetDeviceModels", deviceModelDto);

        return new BaseResult<DeviceModelQueryDto, Paginate<DeviceModelDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<DeviceModel>(),
            Request = deviceModelQueryDto,
            Response = deviceModelDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, DeviceModelDto>> GetDeviceModel(string deviceModelId)
    {
        LogMessage(LogLevel.Information, "In GetDeviceModel", deviceModelId);

        var deviceModel = await _unitOfWork.GetRepository<DeviceModel>()
            .SingleOrDefaultAsync(
                predicate: x => x.DeviceModelId == deviceModelId,
                include: x => x.Include(x => x.DeviceType)
                    .Include(x => x.DeviceFunctions)
                    .ThenInclude(x => x.FunctionParameters)
                    .Include(x => x.DeviceIngredients)
            );

        if (deviceModel is null)
        {
            return new BaseResult<string, DeviceModelDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<DeviceModel>(),
                Request = deviceModelId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var deviceModelDto = _mapper.Map<DeviceModelDto>(deviceModel);

        LogMessage(LogLevel.Information, "Out GetDeviceModel", deviceModelDto);

        return new BaseResult<string, DeviceModelDto>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<DeviceModel>(),
            Request = deviceModelId,
            Response = deviceModelDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<CreateDeviceModelDto, DeviceModelDto>> CreateDeviceModel(
        CreateDeviceModelDto createDeviceModelDto)
    {
        LogMessage(LogLevel.Information, "In CreateDeviceModel", createDeviceModelDto);

        var newDeviceModel = _mapper.Map<DeviceModel>(createDeviceModelDto);

        await _unitOfWork.GetRepository<DeviceModel>().InsertAsync(newDeviceModel);
        var result = await _unitOfWork.CommitAsync();
        LogMessage(LogLevel.Information, "Insert DeviceModel", result);

        var deviceModelDto = _mapper.Map<DeviceModelDto>(newDeviceModel);

        LogMessage(LogLevel.Information, "Out CreateDeviceModel", deviceModelDto);

        return new BaseResult<CreateDeviceModelDto, DeviceModelDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<DeviceModel>(),
            Request = createDeviceModelDto,
            Response = deviceModelDto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateDeviceModelDto, DeviceModelDto>> UpdateDeviceModel(string deviceModelId,
        UpdateDeviceModelDto updateDeviceModelDto)
    {
        // Load device model và các navigation cần thiết
        var deviceModel = await _unitOfWork.GetRepository<DeviceModel>()
            .SingleOrDefaultAsync(
                predicate: x => x.DeviceModelId == deviceModelId,
                include: x => x.Include(x => x.DeviceFunctions)
                    .ThenInclude(x => x.FunctionParameters)
                    .Include(x => x.DeviceIngredients)
            );

        if (deviceModel is null)
        {
            return new BaseResult<UpdateDeviceModelDto, DeviceModelDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<DeviceModel>(),
                Request = updateDeviceModelDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        // Xóa toàn bộ DeviceFunctions (EF tự xóa FunctionParameters nếu quan hệ cascade)
        if (deviceModel.DeviceFunctions is not null && deviceModel.DeviceFunctions.Any())
        {
            _unitOfWork.GetRepository<DeviceFunction>().DeleteRange(deviceModel.DeviceFunctions);
        }

        // Xóa toàn bộ DeviceIngredients
        if (deviceModel.DeviceIngredients is not null && deviceModel.DeviceIngredients.Any())
        {
            _unitOfWork.GetRepository<DeviceIngredient>().DeleteRange(deviceModel.DeviceIngredients);
        }

        // Commit để detach các entity cũ khỏi tracking
        await _unitOfWork.CommitAsync();

        // Map các trường cơ bản, bỏ qua các collection con
        _mapper.Map(updateDeviceModelDto, deviceModel);

        // Tách collection con ra khỏi entity (xử lý riêng)
        var newDeviceFunctions = _mapper.Map<List<DeviceFunction>>(updateDeviceModelDto.DeviceFunctions);
        var newDeviceIngredients = _mapper.Map<List<DeviceIngredient>>(updateDeviceModelDto.DeviceIngredients);

        // Gán null để tránh EF cố update lại các danh sách vừa xóa
        deviceModel.DeviceFunctions = null;
        deviceModel.DeviceIngredients = null;

        // Cập nhật lại DeviceModel trước
        _unitOfWork.GetRepository<DeviceModel>().Update(deviceModel);
        await _unitOfWork.CommitAsync();

        // Gán lại foreign key và quan hệ
        foreach (var func in newDeviceFunctions)
        {
            func.DeviceModelId = deviceModel.DeviceModelId;
            if (func.FunctionParameters != null)
            {
                foreach (var param in func.FunctionParameters)
                {
                    param.DeviceFunction = func;
                }
            }
        }

        foreach (var ingredient in newDeviceIngredients)
        {
            ingredient.DeviceModelId = deviceModel.DeviceModelId;
        }

        // Thêm mới lại các collection con
        if (newDeviceFunctions.Any())
        {
            await _unitOfWork.GetRepository<DeviceFunction>().InsertRangeAsync(newDeviceFunctions);
        }

        if (newDeviceIngredients.Any())
        {
            await _unitOfWork.GetRepository<DeviceIngredient>().InsertRangeAsync(newDeviceIngredients);
        }

        await _unitOfWork.CommitAsync();

        // Map kết quả trả về
        var deviceModelDto = _mapper.Map<DeviceModelDto>(deviceModel);

        return new BaseResult<UpdateDeviceModelDto, DeviceModelDto>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<DeviceModel>(),
            Request = updateDeviceModelDto,
            Response = deviceModelDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }


    public async Task<BaseResult<string, DeviceModelDto>> RemoveDeviceModel(string deviceModelId)
    {
        LogMessage(LogLevel.Information, "In RemoveDeviceModel", deviceModelId);

        var deviceModel = await _unitOfWork.GetRepository<DeviceModel>()
            .SingleOrDefaultAsync(predicate: x => x.DeviceModelId == deviceModelId);

        if (deviceModel is null)
        {
            return new BaseResult<string, DeviceModelDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<DeviceModel>(),
                Request = deviceModelId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var deviceWithDeviceModel = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
            predicate: x => x.DeviceModelId == deviceModelId
        );

        if (deviceWithDeviceModel is not null)
        {
            return new BaseResult<string, DeviceModelDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<DeviceModel>(),
                Request = deviceModelId,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        deviceModel.Delete();

        _unitOfWork.GetRepository<DeviceModel>().Update(deviceModel);
        await _unitOfWork.CommitAsync();

        var deviceModelDto = _mapper.Map<DeviceModelDto>(deviceModel);

        LogMessage(LogLevel.Information, "Out RemoveDeviceModel", deviceModelDto);

        return new BaseResult<string, DeviceModelDto>
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<DeviceModel>(),
            Request = deviceModelId,
            Response = deviceModelDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }
}