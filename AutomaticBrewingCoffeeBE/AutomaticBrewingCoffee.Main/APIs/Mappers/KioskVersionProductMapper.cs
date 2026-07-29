using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.KioskVersionProduct;

namespace AutomaticBrewingCoffee.API.Mappers;

public class KioskVersionProductMapper : Profile
{
    public KioskVersionProductMapper()
    {
        CreateMap<KioskVersionProductDto, KioskVersionProductMapping>()
            .ReverseMap();
        CreateMap<KioskVersionProductInsideDto, KioskVersionProductMapping>()
            .ReverseMap();
        CreateMap<AddKioskVersionProductDto, KioskVersionProductMapping>()
            .ReverseMap();
    }
}