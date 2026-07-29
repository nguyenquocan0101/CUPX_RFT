using Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Base;
using Services.Dtos.Device;
using Domain.Models;
using AutoMapper;
using Domain.Pagination;
using Services.Utils;
using System.Linq.Expressions;
using Services.Dtos.DeviceParameter;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kiosk.ApiService.Converters;
using System.Collections.Concurrent;
using Services.Dtos.DeviceMonitoring;
using Repositories.CouchDbRepository;
using Domain.CouchDbModels;

namespace Services.Implements
{
    public class DeviceService : BaseService<DeviceService>, IDeviceService
    {
        //private readonly CupDroppingMachine _cupDroppingMachine;
        //private readonly IceMachine _iceMachine;
        //private readonly CoffeeMachine _coffeeMachine;
        //private readonly RoboticArm _roboticArm;
        private readonly ConcurrentDictionary<string, Dtos.DeviceMonitoring.DeviceStatus> _deviceStatuses;

        public DeviceService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor
            //CupDroppingMachine cupDroppingMachine,
            //IceMachine iceMachine,
            //CoffeeMachine coffeeMachine,
            //RoboticArm roboticArm
        ) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
        {
            //_cupDroppingMachine = cupDroppingMachine;
            //_iceMachine = iceMachine;
            //_coffeeMachine = coffeeMachine;
            //_roboticArm = roboticArm;
            _deviceStatuses = new ConcurrentDictionary<string, Dtos.DeviceMonitoring.DeviceStatus>();
        }

       

        /// <summary>
        /// Get list of device
        /// </summary>
        /// <param name="deviceQueryDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<DeviceQueryDto, Paginate<DeviceDto>>> GetDevices(DeviceQueryDto deviceQueryDto)
        {
            try
            {
                LogMessage(LogLevel.Information, "In GetDevices", deviceQueryDto);

                var predicate = _unitOfWork.GetRepository<Device>()
                    .BuildSearchPredicate(deviceQueryDto.FilterQuery, deviceQueryDto.FilterBy);

                if (deviceQueryDto.Status is not null)
                {
                    Expression<Func<Device, bool>> statusFilter = x => x.Status == deviceQueryDto.Status ;
                    predicate = ExpressionHelper.CombineExpressions<Device>(predicate, statusFilter);
                }

                var orderBy = _unitOfWork.GetRepository<Device>()
                    .BuildSortingQuery(deviceQueryDto.SortBy, deviceQueryDto.IsAsc);


                var devices = await _unitOfWork.GetRepository<Device>().GetPagingListAsync(
                    predicate: predicate,
                    orderBy: orderBy,
                    page: deviceQueryDto.Page,
                    size: deviceQueryDto.Size
                );

                var deviceDto = _mapper.Map<Paginate<DeviceDto>>(devices);

                LogMessage(LogLevel.Information, "Out GetDevices", deviceDto);

                return new BaseResult<DeviceQueryDto, Paginate<DeviceDto>>()
                {
                    IsSuccess = true,
                    Message = "Devices found.",
                    Request = deviceQueryDto,
                    Response = deviceDto,
                    StatusCode = StatusCodes.Status200OK
                };
            } catch (Exception e)
            {
                return new BaseResult<DeviceQueryDto, Paginate<DeviceDto>>()
                {
                    IsSuccess = true,
                    Message = e.Message,
                    Request = deviceQueryDto,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        /// <summary>
        /// Creates a new device.
        /// </summary>
        /// <param name="createDeviceDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<CreateDeviceDto, DeviceDto>> CreateDevice(CreateDeviceDto createDeviceDto)
        {
            LogMessage(LogLevel.Information, "In CreateDevice", createDeviceDto);

            var newDevice = new Device
            {
                DeviceId = Guid.NewGuid().ToString(),
                Name = createDeviceDto.Name,
                SerialNumber = createDeviceDto.SerialNumber,
                Description = createDeviceDto.Description,
                Status = createDeviceDto.Status
            };

            await _unitOfWork.GetRepository<Device>().InsertAsync(newDevice);
            var isSuccess = await _unitOfWork.CommitAsync() > 0;
            LogMessage(LogLevel.Information, "Insert Device", isSuccess);

            var deviceDto = _mapper.Map<DeviceDto>(newDevice);

            LogMessage(LogLevel.Information, "Out CreateDevice", deviceDto);

            return new BaseResult<CreateDeviceDto, DeviceDto>
            {
                IsSuccess = isSuccess,
                Message = "Device created successfully.",
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
        public async Task<BaseResult<UpdateDeviceDto, DeviceDto>> UpdateDevice(string deviceId, UpdateDeviceDto updateDeviceDto)
        {
            var device = await _unitOfWork.GetRepository<Device>()
                .SingleOrDefaultAsync(predicate: x => x.DeviceId.Equals(deviceId));

            if (device is null)
            {
                return new BaseResult<UpdateDeviceDto, DeviceDto>()
                {
                    IsSuccess = false,
                    Message = "Device not found.",
                    Request = updateDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            device = _mapper.Map(updateDeviceDto, device);
            _unitOfWork.GetRepository<Device>().Update(device);
            var isSuccess = _unitOfWork.Commit() > 0;

            var deviceDto = _mapper.Map<DeviceDto>(device);

            return new BaseResult<UpdateDeviceDto, DeviceDto>()
            {
                IsSuccess = isSuccess,
                Message = "Device updated.",
                Request = updateDeviceDto,
                Response = deviceDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Remove a device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<BaseResult> RemoveDevice(string deviceId)
        {
            LogMessage(LogLevel.Information, "In RemoveDevice", deviceId);

            var device = await _unitOfWork.GetRepository<Device>()
                .SingleOrDefaultAsync(predicate: x => x.DeviceId.Equals(deviceId));

            if (device is null)
            {
                return new BaseResult<string, DeviceDto>()
                {
                    IsSuccess = false,
                    Message = "Device not found.",
                    Request = deviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            _unitOfWork.GetRepository<Device>().Update(device);
            var isSuccess = _unitOfWork.Commit() > 0;

            var deviceDto = _mapper.Map<DeviceDto>(device);

            LogMessage(LogLevel.Information, "Out RemoveDevice", deviceDto);
            return new BaseResult()
            {
                IsSuccess = isSuccess,
                Message = "Device removed.",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, DeviceParameterDto>> GetDeviceParameters(string deviceId)
        {
            try
            {
                var device = await _unitOfWork.GetRepository<Device>()
                    .SingleOrDefaultAsync(predicate: x => x.DeviceId.Equals(deviceId));
                throw new NotImplementedException("This method is not implemented yet for CupDroppingMachine, CoffeeBrewingMachine, RoboticArm");
            }
            catch (Exception ex)
            {
                return new BaseResult<string, DeviceParameterDto>
                {
                    IsSuccess = false,
                    Message = $"Error retrieving device parameters: {ex.Message}",
                    Request = deviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public async Task<BaseResult<SetDeviceParameterDto>> SetDeviceParameters(SetDeviceParameterDto dto)
        {
            try
            {
                List<string> successMessages = new List<string>();
                List<string> errorMessages = new List<string>();

                foreach (var deviceParam in dto.DeviceParamsList)
                {
                    try
                    {
                        var device = await _unitOfWork.GetRepository<Device>()
                            .SingleOrDefaultAsync(predicate: x => x.DeviceId.Equals(deviceParam.DeviceId));

                        if (device == null)
                        {
                            errorMessages.Add($"Device with ID {deviceParam.DeviceId} not found or is deleted.");
                            continue;
                        }

                        //var deviceType = device.DeviceType;
                        //switch (deviceType)
                        //{
                        //    case DeviceType.CupDroppingMachine:
                        //        errorMessages.Add($"Device type {deviceType} for device {deviceParam.DeviceId} is not implemented.");
                        //        break;

                        //    case DeviceType.IceMakerMachine:
                        //        var options = new JsonSerializerOptions
                        //        {
                        //            PropertyNameCaseInsensitive = true,
                        //            ReadCommentHandling = JsonCommentHandling.Disallow,
                        //            AllowTrailingCommas = false,
                        //            DefaultIgnoreCondition = JsonIgnoreCondition.Never
                        //        };
                        //        options.Converters.Add(new RequireAllPropertiesConverter<SetIceMakerMachineParameter>());
                        //        var setIceMachineParameters = JsonSerializer.Deserialize<SetIceMakerMachineParameter>(deviceParam.Parameters, options);
                        //        //var result = _iceMachine.SetParameters(
                        //        //        setIceMachineParameters.Language,
                        //        //        setIceMachineParameters.IceQuantity,
                        //        //        setIceMachineParameters.WaterQuantity,
                        //        //        setIceMachineParameters.IceWaterQuantity);
                        //        var result = true;
                        //        successMessages.Add($"Ice Machine with ID {deviceParam.DeviceId} parameters updated successfully.");
                        //        break;

                        //    case DeviceType.CoffeeBrewingMachine:
                        //        errorMessages.Add($"Device type {deviceType} for device {deviceParam.DeviceId} is not implemented.");
                        //        break;

                        //    case DeviceType.RoboticArm:
                        //        errorMessages.Add($"Device type {deviceType} for device {deviceParam.DeviceId} is not implemented.");
                        //        break;

                        //    default:
                        //        errorMessages.Add($"Device type {deviceType} for device {deviceParam.DeviceId} is not implemented.");
                        //        break;
                        //}
                    }
                    catch (Exception ex)
                    {
                        errorMessages.Add($"Error processing device {deviceParam.DeviceId}: {ex.Message}");
                    }
                }

                string message;
                bool isSuccess;
                int statusCode;

                if (errorMessages.Count == 0)
                {
                    isSuccess = true;
                    message = "All devices parameters updated successfully.";
                    statusCode = StatusCodes.Status202Accepted;
                }
                else if (successMessages.Count == 0)
                {
                    isSuccess = false;
                    message = $"Failed to update all devices parameters: {string.Join(", ", errorMessages)}";
                    statusCode = StatusCodes.Status500InternalServerError;
                }
                else
                {
                    isSuccess = true;
                    message = $"Partially successful: {successMessages.Count} devices updated, {errorMessages.Count} failed. Errors: {string.Join(", ", errorMessages)}";
                    statusCode = StatusCodes.Status207MultiStatus;
                }

                return new BaseResult<SetDeviceParameterDto>
                {
                    IsSuccess = isSuccess,
                    Message = message,
                    StatusCode = statusCode,
                    ResponseRequest = dto,
                };
            }
            catch (Exception ex)
            {
                return new BaseResult<SetDeviceParameterDto>
                {
                    IsSuccess = false,
                    Message = $"Error processing device parameters: {ex.Message}",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ResponseRequest = dto,
                };
            }
        }

        //public DeviceParameterDto MapToDto<T>(T parameters) where T : class
        //{
        //    var dto = new DeviceParameterDto();

        //    switch (parameters)
        //    {
        //        case IceMakerParameterCommand iceMakerParams:
        //            dto.Parameters["CondenserTempCelsius"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.CondenserTempCelsius,
        //                IsSetting = false
        //            };
        //            dto.Parameters["EvaporatorTempCelsius"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.EvaporatorTempCelsius,
        //                IsSetting = false
        //            };
        //            dto.Parameters["AmbientTempCelsius"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.AmbientTempCelsius,
        //                IsSetting = false
        //            };
        //            dto.Parameters["IceQuantity"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.DefaultIceQuantity,
        //                IsSetting = true
        //            };
        //            dto.Parameters["WaterQuantity"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.DefaultWaterQuantity,
        //                IsSetting = true
        //            };
        //            dto.Parameters["IceWaterQuantity"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.DefaultIceWaterQuantity,
        //                IsSetting = true
        //            };
        //            dto.Parameters["Language"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.CurrentLanguage,
        //                IsSetting = true
        //            };
        //            dto.Parameters["VersionNumber"] = new ParameterValue
        //            {
        //                Value = iceMakerParams.VersionNumber,
        //                IsSetting = false
        //            };
        //            break;
        //        default:
        //            throw new ArgumentException($"Unsupported parameter type: {typeof(T).Name}");
        //    }

        //    return dto;
        //}

        public async Task<BaseResult<DeviceDto>> GetDeviceById(string deviceId)
        {
            var device = await _unitOfWork.GetRepository<Device>()
                .SingleOrDefaultAsync(predicate: x => x.DeviceId.Equals(deviceId));

            if (device is null)
            {
                return new BaseResult<DeviceDto>()
                {
                    IsSuccess = false,
                    Message = "Device not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else
            {
                var deviceDto = _mapper.Map<DeviceDto>(device);
                return new BaseResult<DeviceDto>()
                {
                    IsSuccess = true,
                    Message = "Device found.",
                    ResponseRequest = deviceDto,
                    StatusCode = StatusCodes.Status200OK
                };  
            }
        }

        public async Task<BaseResult<IEnumerable<Dtos.DeviceMonitoring.DeviceStatus>>> GetAllDeviceStatusAsync()
        {
            try
            {
                var deviceStatuses = await Task.FromResult(_deviceStatuses.Values.ToList());

                return new BaseResult<IEnumerable<Dtos.DeviceMonitoring.DeviceStatus>>()
                {
                    IsSuccess = true,
                    Message = "All device statuses retrieved successfully.",
                    ResponseRequest = deviceStatuses,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all device statuses");

                return new BaseResult<IEnumerable<Dtos.DeviceMonitoring.DeviceStatus>>()
                {
                    IsSuccess = false,
                    Message = $"Error retrieving device statuses: {ex.Message}",
                    ResponseRequest = Enumerable.Empty<Dtos.DeviceMonitoring.DeviceStatus>(),
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        public async Task UpdateDeviceStatusAsync(Dtos.DeviceMonitoring.DeviceStatus deviceStatus)
        {
            try
            {
                deviceStatus.LastChecked = DateTime.UtcNow;
                _deviceStatuses.AddOrUpdate(deviceStatus.DeviceId, deviceStatus, (key, oldValue) => deviceStatus);

                _logger.LogDebug("Updated status for device {DeviceId}: {Status}",
                    deviceStatus.DeviceId, deviceStatus.IsConnected ? "Connected" : "Disconnected");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device status for {DeviceId}", deviceStatus.DeviceId);
            }
        }

        public async Task<BaseResult> UpdateDeviceCoordinates(string deviceId, UpdateDeviceCoordinateDto request)
        {
            var device = await _unitOfWork.GetRepository<Device>()
                .SingleOrDefaultAsync(predicate: x => x.DeviceId.Equals(deviceId));
            if (device is null)
            {
                return new BaseResult()
                {
                    IsSuccess = false,
                    Message = "Device not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            try
            {
                device.X = request.X ?? device.X;
                device.Y = request.Y ?? device.Y;
                device.Z = request.Z ?? device.Z;
                device.RX = request.RX ?? device.RX;
                device.RY = request.RY ?? device.RY;
                device.RZ = request.RZ ?? device.RZ;
                device.J1 = request.J1 ?? device.J1;
                device.J2 = request.J2 ?? device.J2;
                device.J3 = request.J3 ?? device.J3;
                device.J4 = request.J4 ?? device.J4;
                device.J5 = request.J5 ?? device.J5;
                device.J6 = request.J6 ?? device.J6;

                _unitOfWork.GetRepository<Device>().Update(device);
                var isSuccess = await _unitOfWork.CommitAsync() > 0;

                return new BaseResult()
                {
                    IsSuccess = isSuccess,
                    Message = "Device coordinates updated successfully.",
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResult()
                {
                    IsSuccess = false,
                    Message = $"Error updating device coordinates: {ex.Message}",
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}