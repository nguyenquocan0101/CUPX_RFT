using Domain.CouchDbModels;
using Domain.Pagination;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.DeviceMonitoring;
using Services.Dtos.DeviceParameter;

namespace Services.Interfaces
{
    public interface IDeviceService
    {
        Task<BaseResult<DeviceQueryDto, Paginate<DeviceDto>>> GetDevices(DeviceQueryDto deviceQueryDto);
        Task<BaseResult<DeviceDto>> GetDeviceById(string deviceId);
        Task<BaseResult<string, DeviceParameterDto>> GetDeviceParameters(string deviceId);
        Task<BaseResult<SetDeviceParameterDto>> SetDeviceParameters(SetDeviceParameterDto deviceQueryDto);
        Task<BaseResult<CreateDeviceDto, DeviceDto>> CreateDevice(CreateDeviceDto createDeviceDto);
        Task<BaseResult<UpdateDeviceDto, DeviceDto>> UpdateDevice(string deviceId, UpdateDeviceDto updateDeviceDto);
        Task<BaseResult> RemoveDevice(string deviceId);
        //Task<BaseResult<IEnumerable<DeviceStatus>>> GetAllDeviceStatusAsync();
        //Task UpdateDeviceStatusAsync(DeviceStatus deviceStatus);
        Task<BaseResult> UpdateDeviceCoordinates(string deviceId, UpdateDeviceCoordinateDto request);
    }
}