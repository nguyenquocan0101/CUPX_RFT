using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.DeviceIngredientHistory;

namespace AutomaticBrewingCoffee.API.Mappers;

public class DeviceIngredientHistoryMapper : Profile
{
    public DeviceIngredientHistoryMapper()
    {
        CreateMap<DeviceIngredientHistory, DeviceIngredientHistoryInsideDto>().ReverseMap();
        CreateMap<DeviceIngredientHistory, DeviceIngredientHistoryDto>().ReverseMap();
    }
}