using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.DeviceIngredient;
using Services.Dtos.DeviceIngredientState;

namespace AutomaticBrewingCoffee.API.Mappers;

public class DeviceIngredientStateMapper : Profile
{
    public DeviceIngredientStateMapper()
    {
        CreateMap<DeviceIngredientStateNestedDto, DeviceIngredientState>().ReverseMap();
        CreateMap<DeviceIngredientStateInsideDto, DeviceIngredientState>().ReverseMap();
        CreateMap<DeviceIngredientStateDto, DeviceIngredientState>().ReverseMap();

        CreateMap<UpdateDeviceIngredientStateDto, DeviceIngredientState>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastRefilledDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();
    }
}