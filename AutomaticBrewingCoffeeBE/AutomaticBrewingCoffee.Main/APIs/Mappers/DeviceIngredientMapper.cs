using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.DeviceIngredient;

namespace AutomaticBrewingCoffee.API.Mappers;

public class DeviceIngredientMapper : Profile
{
    public DeviceIngredientMapper()
    {
        CreateMap<DeviceIngredientDto, DeviceIngredient>().ReverseMap();

        CreateMap<DeviceIngredientInsideDto, DeviceIngredient>().ReverseMap();

        CreateMap<DeviceIngredientNestedDto, DeviceIngredient>()
            .ForMember(dest => dest.DeviceIngredientId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap();
    }
}