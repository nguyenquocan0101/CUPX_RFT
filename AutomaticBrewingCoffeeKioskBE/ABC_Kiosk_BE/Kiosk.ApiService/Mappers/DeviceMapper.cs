using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Services.Dtos.Device;

namespace Kiosk.ApiService.Mappers
{
    public class DeviceMapper : Profile
    {
        public DeviceMapper()
        {
            CreateMap<CreateDeviceDto, Device>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<UpdateDeviceDto, Device>()
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
               .ReverseMap();
            CreateMap<Device, DeviceDto>()
             .ReverseMap();
        }
    }
}
