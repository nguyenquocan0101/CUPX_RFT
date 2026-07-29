using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.DeviceIngredientState;

namespace Services.Interfaces
{
    public interface IDeviceService
    {
        Task<BaseResult<DeviceQueryDto, Paginate<DeviceDto>>> GetDevices(DeviceQueryDto deviceQueryDto);
        Task<BaseResult<string, DeviceDto>> GetDevice(string deviceId);
        Task<BaseResult<CreateDeviceDto, DeviceDto>> CreateDevice(CreateDeviceDto createDeviceDto);
        Task<BaseResult<UpdateDeviceDto, DeviceDto>> UpdateDevice(string deviceId, UpdateDeviceDto updateDeviceDto);
        Task<BaseResult<string, DeviceDto>> RemoveDevice(string deviceId);

        Task<BaseResult<DeviceQueryDto, Paginate<DeviceDto>>> GetDeviceReplace(string deviceId,
            DeviceQueryDto deviceQueryDto);

        Task<BaseResult<UpdateDeviceIngredientStateDto, DeviceIngredientStateDto>> UpdateDeviceIngredientState(
            string deviceIngredientStateId,
            UpdateDeviceIngredientStateDto updateDeviceIngredientStateDto
        );
    }
}