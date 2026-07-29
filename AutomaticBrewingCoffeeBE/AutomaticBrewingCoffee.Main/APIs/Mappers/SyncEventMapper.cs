using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.SyncEvent;

namespace AutomaticBrewingCoffee.API.Mappers;

public class SyncEventMapper : Profile
{
    public SyncEventMapper()
    {
        CreateMap<IPaginate<SyncEventDto>, IPaginate<SyncEvent>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<SyncEvent, SyncEventDto>()
            .ReverseMap();
    }
}