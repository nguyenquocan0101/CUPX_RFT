using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.DeviceFunction;

namespace AutomaticBrewingCoffee.API.Mappers;

public class DeviceFunctionMapper : Profile
{
    public DeviceFunctionMapper()
    {
        CreateMap<CreateDeviceFunctionDto, DeviceFunctionDto>()
            .ReverseMap();

        CreateMap<DeviceFunctionNestedDto, DeviceFunction>()
            .ForMember(dest => dest.DeviceFunctionId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .AfterMap((src, dest) =>
            {
                var functionParameters = dest.FunctionParameters;

                if (functionParameters == null)
                {
                    return;
                }

                var deviceFunctionId = dest.DeviceFunctionId;

                foreach (var functionParameter in functionParameters)
                {
                    functionParameter.DeviceFunctionId = deviceFunctionId;
                }
            })
            .ReverseMap();

        CreateMap<CreateDeviceFunctionDto, DeviceFunction>()
            .ForMember(dest => dest.DeviceFunctionId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateDeviceFunctionDto, DeviceFunction>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<DeviceFunctionDto>, IPaginate<DeviceFunction>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<DeviceFunction, DeviceFunctionDto>()
            .ReverseMap();

        CreateMap<DeviceFunction, DeviceFunctionInsideDto>()
            .ReverseMap();
    }
}