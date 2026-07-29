using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.SyncTask;

namespace AutomaticBrewingCoffee.API.Mappers;

public class SyncTaskMapper : Profile
{
    public SyncTaskMapper()
    {
        CreateMap<IPaginate<SyncTaskDto>, IPaginate<SyncTask>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<SyncTask, SyncTaskDto>()
            .ReverseMap();
        CreateMap<SyncTask, SyncTaskInSyncEventDto>()
            .ReverseMap();
    }
}