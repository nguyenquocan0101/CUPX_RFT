using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Kiosk;
using Services.Dtos.KioskDevice;

namespace AutomaticBrewingCoffee.API.Mappers;

public class KioskDeviceMapper : Profile
{
    public KioskDeviceMapper()
    {
        CreateMap<KioskDeviceDto, KioskDeviceMapping>()
            .ReverseMap();
        
        CreateMap<KioskDeviceInsideDto, KioskDeviceMapping>()
            .ReverseMap();
    }
}