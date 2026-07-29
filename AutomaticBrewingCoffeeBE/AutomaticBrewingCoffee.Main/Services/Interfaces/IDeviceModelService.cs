using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.DeviceModel;

namespace Services.Interfaces;

public interface IDeviceModelService
{
    Task<BaseResult<DeviceModelQueryDto, Paginate<DeviceModelDto>>> GetDeviceModels(DeviceModelQueryDto deviceModelQueryDto);
    Task<BaseResult<string, DeviceModelDto>> GetDeviceModel(string deviceModelId);
    Task<BaseResult<CreateDeviceModelDto, DeviceModelDto>> CreateDeviceModel(CreateDeviceModelDto createDeviceModelDto);
    Task<BaseResult<UpdateDeviceModelDto, DeviceModelDto>> UpdateDeviceModel(string deviceModelId, UpdateDeviceModelDto updateDeviceModelDto);
    Task<BaseResult<string, DeviceModelDto>> RemoveDeviceModel(string deviceModelId);
}