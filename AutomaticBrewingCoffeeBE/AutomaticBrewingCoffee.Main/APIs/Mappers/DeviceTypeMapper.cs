using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.DeviceType;

namespace AutomaticBrewingCoffee.API.Mappers;

public class DeviceTypeMapper : Profile
{
    public DeviceTypeMapper()
    {
        CreateMap<CreateDeviceTypeDto, DeviceTypeDto>()
            .ReverseMap();

        CreateMap<CreateDeviceTypeDto, DeviceType>()
            .ForMember(dest => dest.DeviceTypeId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateDeviceTypeDto, DeviceType>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<DeviceTypeDto>, IPaginate<DeviceType>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<DeviceType, DeviceTypeDto>()
            .ReverseMap();
    }
}