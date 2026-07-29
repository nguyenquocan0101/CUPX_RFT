using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.DeviceType;

namespace Services.Interfaces;

public interface IDeviceTypeService
{
    Task<BaseResult<DeviceTypeQueryDto, Paginate<DeviceTypeDto>>> GetDeviceTypes(DeviceTypeQueryDto deviceTypeQueryDto);
    Task<BaseResult<string, DeviceTypeDto>> GetDeviceType(string deviceTypeId);
    Task<BaseResult<CreateDeviceTypeDto, DeviceTypeDto>> CreateDeviceType(CreateDeviceTypeDto createDeviceTypeDto);

    Task<BaseResult<UpdateDeviceTypeDto, DeviceTypeDto>> UpdateDeviceType(string deviceTypeId,
        UpdateDeviceTypeDto updateDeviceTypeDto);

    Task<BaseResult<string, DeviceTypeDto>> RemoveDeviceType(string deviceTypeId);
}