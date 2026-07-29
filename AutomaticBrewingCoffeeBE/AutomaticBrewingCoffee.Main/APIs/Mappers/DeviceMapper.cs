using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.Device;

namespace AutomaticBrewingCoffee.API.Mappers
{
    public class DeviceMapper : Profile
    {
        public DeviceMapper()
        {
            CreateMap<CreateDeviceDto, DeviceDto>()
                .ReverseMap();

            CreateMap<CreateDeviceDto, Device>()
                .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsOnHub, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UpdateDeviceDto, Device>()
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<IPaginate<DeviceDto>, IPaginate<Device>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
                .ReverseMap();

            CreateMap<Device, DeviceDto>()
                .ReverseMap();

            CreateMap<Device, DeviceInsideDto>()
                .ForMember(dest => dest.DeviceIngredientHistories, opt => opt.MapFrom(src =>
                    (src.DeviceIngredientHistories ?? Array.Empty<DeviceIngredientHistory>())
                    .OrderByDescending(h => h.CreatedDate)
                    .ThenByDescending(h => h.DeviceIngredientHistoryId)
                ))
                .ReverseMap();
        }
    }
}