using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.KioskVersion;

namespace AutomaticBrewingCoffee.API.Mappers;

public class KioskVersionMapper : Profile
{
    public KioskVersionMapper()
    {
        CreateMap<CreateKioskVersionDto, KioskVersionDto>()
            .ReverseMap();

        CreateMap<CreateKioskVersionDto, KioskVersion>()
            .ForMember(dest => dest.KioskVersionId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateKioskVersionDto, KioskVersion>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<KioskVersionDto>, IPaginate<KioskVersion>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<KioskVersion, KioskVersionDto>()
            .ReverseMap();

        CreateMap<KioskVersion, KioskVersionInsideDto>()
            .ReverseMap();
    }
}