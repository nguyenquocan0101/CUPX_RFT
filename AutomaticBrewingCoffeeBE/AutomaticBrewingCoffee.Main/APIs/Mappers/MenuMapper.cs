using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Menu;
using Services.Dtos.MenuProduct;

namespace AutomaticBrewingCoffee.API.Mappers;

public class MenuMapper : Profile
{
    public MenuMapper()
    {
        CreateMap<CreateMenuDto, Menu>()
            .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap();

        CreateMap<UpdateMenuDto, Menu>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<MenuDto, Menu>()
            .ReverseMap();
        
        CreateMap<MenuInsideDto, Menu>()
            .ReverseMap();

        CreateMap<MenuForKioskDto, Menu>()
            .ReverseMap();

        CreateMap<Menu, Menu>()
            .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap()
            .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .AfterMap((src, dest, context) =>
            {
                if (dest.MenuProductMappings is null)
                {
                    return;
                }

                foreach (var menuProductMapping in dest.MenuProductMappings)
                {
                    menuProductMapping.MenuId = dest.MenuId;
                }
            });
    }
}