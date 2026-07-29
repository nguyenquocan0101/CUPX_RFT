using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.IngredientType;

namespace AutomaticBrewingCoffee.API.Mappers;

public class IngredientTypeMapper : Profile
{
    public IngredientTypeMapper()
    {
        CreateMap<CreateIngredientTypeDto, IngredientTypeDto>()
            .ReverseMap();

        CreateMap<CreateIngredientTypeDto, IngredientType>()
            .ForMember(dest => dest.IngredientTypeId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateIngredientTypeDto, IngredientType>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<IngredientTypeDto>, IPaginate<IngredientType>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<IngredientType, IngredientTypeDto>()
            .ReverseMap();
    }
}