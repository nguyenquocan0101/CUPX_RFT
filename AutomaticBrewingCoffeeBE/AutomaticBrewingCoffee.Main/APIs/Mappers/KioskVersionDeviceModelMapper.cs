using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.KioskVersionDeviceModel;

namespace AutomaticBrewingCoffee.API.Mappers;

public class KioskVersionDeviceModelMapper : Profile
{
    public KioskVersionDeviceModelMapper()
    {
        CreateMap<KioskVersionDeviceModelDto, KioskVersionDeviceModelMapping>()
            .ReverseMap();
        CreateMap<KioskVersionDeviceModelInsideDto, KioskVersionDeviceModelMapping>()
            .ReverseMap();
        CreateMap<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelMapping>()
            .ReverseMap();
    }
}