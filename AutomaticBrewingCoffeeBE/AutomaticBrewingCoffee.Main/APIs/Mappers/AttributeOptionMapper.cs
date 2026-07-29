using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.AttributeOption;

namespace AutomaticBrewingCoffee.API.Mappers;

public class AttributeOptionMapper : Profile
{
    public AttributeOptionMapper()
    {
        CreateMap<AttributeOptionInsideDto, AttributeOption>()
            .ReverseMap();

        CreateMap<AttributeOptionNestedDto, AttributeOption>()
            .ForMember(dest => dest.AttributeOptionId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<AttributeOption, AttributeOption>()
            .ForMember(dest => dest.AttributeOptionId,
                opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap()
            .ForMember(dest => dest.AttributeOptionId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());
    }
}