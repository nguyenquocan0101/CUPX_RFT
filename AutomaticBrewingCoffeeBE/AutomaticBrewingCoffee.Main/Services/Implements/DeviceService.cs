using AutomaticBrewingCoffee.Repository.Interfaces;
using Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Device;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using System.Linq.Expressions;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Services.AzureIotHub;
using Services.Dtos.DeviceIngredientState;
using Services.Utils;

namespace Services.Implements
{
    public class DeviceService : BaseService<DeviceService>, IDeviceService
    {
        private DeviceManager _deviceManager;

        public DeviceService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor,
            DeviceManager deviceManager
        ) : base(
            unitOfWork,
            mapper,
            loggerFactory,
            httpContextAccessor
        )
        {
            _deviceManager = deviceManager;
        }

        /// <summary>
        /// Get list of device
        /// </summary>
        /// <param name="deviceQueryDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<DeviceQueryDto, Paginate<DeviceDto>>> GetDevices(DeviceQueryDto deviceQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetDevices", deviceQueryDto);

            var predicate = _unitOfWork.GetRepository<Device>()
                .BuildSearchPredicate(deviceQueryDto.FilterQuery, deviceQueryDto.FilterBy);

            Expression<Func<Device, bool>> isDeletedFilter = x =>
                x.IsDeleted == false;
            predicate = ExpressionHelper.CombineExpressions<Device>(predicate, isDeletedFilter);


            if (deviceQueryDto.StartDate is not null && deviceQueryDto.EndDate is not null)
            {
                var dateRangePredicate = _unitOfWork.GetRepository<Device>().BuildDateRangePredicate(
                    deviceQueryDto.StartDate,
                    deviceQueryDto.EndDate
                );
                predicate = ExpressionHelper.CombineExpressions<Device>(predicate, dateRangePredicate);
            }

            if (deviceQueryDto.Status is not null)
            {
                Expression<Func<Device, bool>> statusFilter = x => x.Status == deviceQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Device>(predicate, statusFilter);
            }

            if (deviceQueryDto.DeviceModelId is not null)
            {
                Expression<Func<Device, bool>> statusFilter = x => x.DeviceModelId == deviceQueryDto.DeviceModelId;
                predicate = ExpressionHelper.CombineExpressions<Device>(predicate, statusFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Device>()
                .BuildSortingQuery(deviceQueryDto.SortBy, deviceQueryDto.IsAsc);

            var devices = await _unitOfWork.GetRepository<Device>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: deviceQueryDto.Page,
                size: deviceQueryDto.Size,
                include: x => x
                    .Include(x => x.DeviceModel)
                    .ThenInclude(x => x.DeviceType)
                    .Include(x => x.DeviceIngredientStates)
            );

            var deviceDto = _mapper.Map<Paginate<DeviceDto>>(devices);

            LogMessage(LogLevel.Information, "Out GetDevices", deviceDto);

            return new BaseResult<DeviceQueryDto, Paginate<DeviceDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Device>(),
                Request = deviceQueryDto,
                Response = deviceDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Get a device by id.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<BaseResult<string, DeviceDto>> GetDevice(string deviceId)
        {
            LogMessage(LogLevel.Information, "In GetDevice", deviceId);

            var device = await _unitOfWork.GetRepository<Device>()
                .SingleOrDefaultAsync(
                    predicate: x => x.DeviceId == deviceId,
                    include: x => x
                        .Include(x => x.DeviceModel)
                        .ThenInclude(x => x.DeviceType)
                        .Include(x => x.DeviceIngredientStates)
                );

            if (device is null)
            {
                return new BaseResult<string, DeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = deviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var deviceDto = _mapper.Map<DeviceDto>(device);

            LogMessage(LogLevel.Information, "Out GetDevice", deviceDto);

            return new BaseResult<string, DeviceDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Device>(),
                Request = deviceId,
                Response = deviceDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Creates a new device.
        /// </summary>
        /// <param name="createDeviceDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<CreateDeviceDto, DeviceDto>> CreateDevice(CreateDeviceDto createDeviceDto)
        {
            LogMessage(LogLevel.Information, "In CreateDevice", createDeviceDto);

            var device = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                predicate: x => x.SerialNumber.Trim() == createDeviceDto.SerialNumber.Trim()
            );

            if (device is not null)
            {
                return new BaseResult<CreateDeviceDto, DeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.AlreadyExists<Device>(device.SerialNumber),
                    Request = createDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var deviceModel = await _unitOfWork.GetRepository<DeviceModel>().SingleOrDefaultAsync(
                predicate: x => x.DeviceModelId == createDeviceDto.DeviceModelId,
                include: x => x.Include(x => x.DeviceIngredients)
            );

            if (deviceModel is null)
            {
                return new BaseResult<CreateDeviceDto, DeviceDto>()
                {
                    IsSuccess = true,
                    Message = MessageUtil.NotFound<DeviceModel>(),
                    Request = createDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var newDevice = _mapper.Map<Device>(createDeviceDto);

            await _unitOfWork.GetRepository<Device>().InsertAsync(newDevice);

            // Auto seeding device ingredient states for new device
            if (deviceModel.DeviceIngredients != null)
            {
                var deviceIngredientStates = deviceModel.DeviceIngredients.Select(x => new DeviceIngredientState()
                {
                    DeviceIngredientStateId = Guid.NewGuid().ToString(),
                    DeviceId = newDevice.DeviceId,
                    CurrentCapacity = 0,
                    IngredientType = x.IngredientType,
                    MaxCapacity = x.MaxCapacity,
                    MinCapacity = x.MinCapacity,
                    IsWarning = true,
                    WarningPercent = x.WarningPercent,
                    IsPrimary = x.IsPrimary,
                    IsRenewable = x.IsRenewable,
                    CapacityLevel = ECapacityLevel.Low.ToString(),
                    IsDeleted = false,
                    Unit = x.Unit,
                    CreatedDate = DateTime.UtcNow,
                    LastRefilledDate = null,
                    DeletedDate = null,
                    UpdatedDate = null
                }).ToList();
                newDevice.DeviceIngredientStates = deviceIngredientStates;
            }

            var result = await _unitOfWork.CommitAsync();
            LogMessage(LogLevel.Information, "Insert Device", result);

            var deviceDto = _mapper.Map<DeviceDto>(newDevice);

            LogMessage(LogLevel.Information, "Out CreateDevice", deviceDto);

            return new BaseResult<CreateDeviceDto, DeviceDto>
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<Device>(),
                Request = createDeviceDto,
                Response = deviceDto,
                StatusCode = StatusCodes.Status201Created
            };
        }

        /// <summary>
        /// Update a device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="updateDeviceDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<UpdateDeviceDto, DeviceDto>> UpdateDevice(string deviceId,
            UpdateDeviceDto updateDeviceDto)
        {
            var device = await _unitOfWork.GetRepository<Device>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.DeviceId == deviceId
                );

            if (device is null)
            {
                return new BaseResult<UpdateDeviceDto, DeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = updateDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (device.SerialNumber != updateDeviceDto.SerialNumber)
            {
                var deviceWithSerial = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                    predicate: x => x.SerialNumber.Trim() == updateDeviceDto.SerialNumber.Trim()
                );

                if (deviceWithSerial is not null)
                {
                    return new BaseResult<UpdateDeviceDto, DeviceDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.AlreadyExists<Device>(deviceWithSerial.SerialNumber),
                        Request = updateDeviceDto,
                        Response = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
            }

            var deviceInKiosk = await _unitOfWork.GetRepository<KioskDeviceMapping>().SingleOrDefaultAsync(
                predicate: x => x.DeviceId == deviceId && x.IsDisposed == false);

            if (deviceInKiosk is not null)
            {
                if (updateDeviceDto.Status != EDeviceStatus.Working.ToString())
                {
                    return new BaseResult<UpdateDeviceDto, DeviceDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.DeviceStatusError(Enum.Parse<EDeviceStatus>(device.Status)),
                        Request = updateDeviceDto,
                        Response = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
            }

            device = _mapper.Map(updateDeviceDto, device);

            _unitOfWork.GetRepository<Device>().Update(device);
            await _unitOfWork.CommitAsync();

            var deviceDto = _mapper.Map<DeviceDto>(device);

            return new BaseResult<UpdateDeviceDto, DeviceDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Device>(),
                Request = updateDeviceDto,
                Response = deviceDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        /// <summary>
        /// Remove a device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<BaseResult<string, DeviceDto>> RemoveDevice(string deviceId)
        {
            LogMessage(LogLevel.Information, "In RemoveDevice", deviceId);

            var device = await _unitOfWork.GetRepository<Device>()
                .SingleOrDefaultAsync(predicate: x => x.DeviceId == deviceId);

            if (device is null)
            {
                return new BaseResult<string, DeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = deviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (!device.Status.Equals(nameof(EDeviceStatus.Stock)))
            {
                return new BaseResult<string, DeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.DeviceStatusError(Enum.Parse<EDeviceStatus>(device.Status)),
                    Request = deviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            device.Delete();
            device.DownHub();

            await _deviceManager.RemoveHubDevice(device.DeviceId);

            _unitOfWork.GetRepository<Device>().Update(device);
            await _unitOfWork.CommitAsync();

            var deviceDto = _mapper.Map<DeviceDto>(device);

            LogMessage(LogLevel.Information, "Out RemoveDevice", deviceDto);
            return new BaseResult<string, DeviceDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.DeleteSuccess<Device>(),
                Request = deviceId,
                Response = deviceDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }


        public async Task<BaseResult<DeviceQueryDto, Paginate<DeviceDto>>> GetDeviceReplace(string deviceId,
            DeviceQueryDto deviceQueryDto)
        {
            deviceQueryDto.Status = EDeviceStatus.Stock.ToString();

            LogMessage(LogLevel.Information, "In GetReplaceDevice", deviceQueryDto);

            var predicate = _unitOfWork.GetRepository<Device>()
                .BuildSearchPredicate(deviceQueryDto.FilterQuery, deviceQueryDto.FilterBy);

            Expression<Func<Device, bool>> isDeletedFilter = x =>
                x.IsDeleted == false;
            predicate = ExpressionHelper.CombineExpressions<Device>(predicate, isDeletedFilter);

            var device = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                predicate: x => x.DeviceId == deviceId
            );

            if (device is null)
            {
                return new BaseResult<DeviceQueryDto, Paginate<DeviceDto>>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = deviceQueryDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            deviceQueryDto.DeviceModelId = device.DeviceModelId;

            Expression<Func<Device, bool>> isExtinct = x =>
                x.DeviceId != device.DeviceId;

            predicate = ExpressionHelper.CombineExpressions<Device>(predicate, isExtinct);

            if (deviceQueryDto.Status is not null)
            {
                Expression<Func<Device, bool>> statusFilter = x => x.Status == deviceQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Device>(predicate, statusFilter);
            }

            if (deviceQueryDto.DeviceModelId is not null)
            {
                Expression<Func<Device, bool>> statusFilter = x => x.DeviceModelId == deviceQueryDto.DeviceModelId;
                predicate = ExpressionHelper.CombineExpressions<Device>(predicate, statusFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Device>()
                .BuildSortingQuery(deviceQueryDto.SortBy, deviceQueryDto.IsAsc);

            var devices = await _unitOfWork.GetRepository<Device>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: deviceQueryDto.Page,
                size: deviceQueryDto.Size,
                include: x => x
                    .Include(x => x.DeviceModel)
                    .ThenInclude(x => x.DeviceType)
            );

            var deviceDto = _mapper.Map<Paginate<DeviceDto>>(devices);

            LogMessage(LogLevel.Information, "Out GetReplaceDevice", deviceDto);

            return new BaseResult<DeviceQueryDto, Paginate<DeviceDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Device>(),
                Request = deviceQueryDto,
                Response = deviceDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<UpdateDeviceIngredientStateDto, DeviceIngredientStateDto>>
            UpdateDeviceIngredientState(
                string deviceIngredientStateId,
                UpdateDeviceIngredientStateDto updateDeviceIngredientStateDto)
        {
            var kioskId = GetKioskIdFromJwt();

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskId,
                include: x => x.Include(x => x.KioskDevices)
            );

            if (kiosk is null)
            {
                return new BaseResult<UpdateDeviceIngredientStateDto, DeviceIngredientStateDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = updateDeviceIngredientStateDto,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var kioskDeviceIds = kiosk.KioskDevices.Select(x => x.DeviceId);

            var deviceIngredientState = await _unitOfWork.GetRepository<DeviceIngredientState>()
                .SingleOrDefaultAsync(
                    predicate: x => x.DeviceIngredientStateId == deviceIngredientStateId
                );

            if (deviceIngredientState is null)
            {
                return new BaseResult<UpdateDeviceIngredientStateDto, DeviceIngredientStateDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<DeviceIngredientState>(),
                    Request = updateDeviceIngredientStateDto,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            if (!kioskDeviceIds.Contains(deviceIngredientState.DeviceId))
            {
                return new BaseResult<UpdateDeviceIngredientStateDto, DeviceIngredientStateDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<DeviceIngredientState>(),
                    Request = updateDeviceIngredientStateDto,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var deviceIngredientHistory = new DeviceIngredientHistory()
            {
                DeviceIngredientHistoryId = Guid.NewGuid().ToString(),
                DeviceIngredientStateId = deviceIngredientState.DeviceIngredientStateId,
                DeviceId = deviceIngredientState.DeviceId,
                IngredientType = deviceIngredientState.IngredientType,
                Action = EIngredientAction.Refill.ToString(),
                PerformedBy = "Staff",
                DeltaAmount = updateDeviceIngredientStateDto.CurrentCapacity - deviceIngredientState.CurrentCapacity,
                OldCapacity = deviceIngredientState.CurrentCapacity,
                NewCapacity = updateDeviceIngredientStateDto.CurrentCapacity
            };

            deviceIngredientState = _mapper.Map(updateDeviceIngredientStateDto, deviceIngredientState);
            deviceIngredientState.Recalculate();

            _unitOfWork.GetRepository<DeviceIngredientState>().Update(deviceIngredientState);
            await _unitOfWork.GetRepository<DeviceIngredientHistory>().InsertAsync(deviceIngredientHistory);
            await _unitOfWork.CommitAsync();

            var deviceIngredientStateDto = _mapper.Map<DeviceIngredientStateDto>(deviceIngredientState);

            return new BaseResult<UpdateDeviceIngredientStateDto, DeviceIngredientStateDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<DeviceIngredientState>(),
                Request = updateDeviceIngredientStateDto,
                StatusCode = StatusCodes.Status202Accepted,
                Response = deviceIngredientStateDto
            };
        }
    }
}