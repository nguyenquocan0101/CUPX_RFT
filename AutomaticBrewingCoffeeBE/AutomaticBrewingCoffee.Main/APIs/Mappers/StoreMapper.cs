using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.Store;

namespace AutomaticBrewingCoffee.API.Mappers
{
    public class StoreMapper : Profile
    {
        public StoreMapper()
        {
            CreateMap<CreateStoreDto, StoreDto>()
                .ReverseMap();

            CreateMap<CreateStoreDto, Store>()
                .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<UpdateStoreDto, Store>()
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<IPaginate<StoreDto>, IPaginate<Store>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
                .ReverseMap();

            CreateMap<Store, StoreDto>()
                .ReverseMap();

            CreateMap<Store, StoreInsideDto>()
                .ReverseMap();

            CreateMap<Store, StoreReverseDto>()
                .ReverseMap();
        }
    }
}