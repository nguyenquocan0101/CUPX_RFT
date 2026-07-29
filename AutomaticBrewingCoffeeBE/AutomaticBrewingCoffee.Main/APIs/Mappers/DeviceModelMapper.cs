using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.DeviceModel;

namespace AutomaticBrewingCoffee.API.Mappers;

public class DeviceModelModelMapper : Profile
{
    public DeviceModelModelMapper()
    {
        CreateMap<CreateDeviceModelDto, DeviceModelDto>()
            .ReverseMap();

        CreateMap<CreateDeviceModelDto, DeviceModel>()
            .ForMember(dest => dest.DeviceModelId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                var deviceFunctions = dest.DeviceFunctions;

                if (deviceFunctions == null)
                {
                    return;
                }

                var deviceModelId = dest.DeviceModelId;

                foreach (var deviceFunction in deviceFunctions)
                {
                    deviceFunction.DeviceModelId = deviceModelId;
                }

                var deviceIngredients = dest.DeviceIngredients;

                if (deviceIngredients == null)
                {
                    return;
                }

                foreach (var deviceIngredient in deviceIngredients)
                {
                    deviceIngredient.DeviceModelId = deviceModelId;
                }
            })
            .ReverseMap();

        CreateMap<UpdateDeviceModelDto, DeviceModel>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<DeviceModelDto>, IPaginate<DeviceModel>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<DeviceModel, DeviceModelDto>()
            .ReverseMap();

        CreateMap<DeviceModel, DeviceModelInsideDto>()
            .ReverseMap();
    }
}