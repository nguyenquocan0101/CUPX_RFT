using AutoMapper;
using Domain.Enums;
using Domain.Models;
using Services.Dtos.Device;
using Services.Dtos.Menu;

namespace Kiosk.ApiService.Mappers
{
    public class MenuMapper : Profile
    {
        public MenuMapper()
        {
            CreateMap<Menu, MenuDto>()
                .ForMember(dest => dest.ProductsInMenu, opt => opt.MapFrom(src => src.MenuProductMappings))
                .ReverseMap();
            CreateMap<CreateMenuDto, Menu>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<UpdateMenuDto, Menu>()
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
               .ReverseMap();
            CreateMap<MenuProductMapping, MenuProductMappingDto>().ReverseMap();
        }
    }
}
