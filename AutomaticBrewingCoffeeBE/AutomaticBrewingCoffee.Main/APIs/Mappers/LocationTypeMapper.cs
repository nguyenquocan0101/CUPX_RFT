using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.LocationType;

namespace AutomaticBrewingCoffee.API.Mappers;

public class LocationTypeMapper : Profile
{
    public LocationTypeMapper()
    {
        
        CreateMap<CreateLocationTypeDto, LocationTypeDto>()
            .ReverseMap();

        CreateMap<CreateLocationTypeDto, LocationType>()
            .ForMember(dest => dest.LocationTypeId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateLocationTypeDto, LocationType>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<LocationTypeDto>, IPaginate<LocationType>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<LocationType, LocationTypeDto>()
            .ReverseMap();
    }
}