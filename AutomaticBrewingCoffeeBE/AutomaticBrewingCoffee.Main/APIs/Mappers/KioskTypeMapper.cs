using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.KioskType;

namespace AutomaticBrewingCoffee.API.Mappers;

public class KioskTypeMapper : Profile
{
    public KioskTypeMapper()
    {
        CreateMap<CreateKioskTypeDto, KioskTypeDto>()
            .ReverseMap();

        CreateMap<CreateKioskTypeDto, KioskType>()
            .ForMember(dest => dest.KioskTypeId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateKioskTypeDto, KioskType>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<KioskTypeDto>, IPaginate<KioskType>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<KioskType, KioskTypeDto>()
            .ReverseMap();
    }
}