using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.MenuProduct;

namespace AutomaticBrewingCoffee.API.Mappers;

public class MenuProductMapper : Profile
{
    public MenuProductMapper()
    {
        CreateMap<MenuProductMappingDto, MenuProductMapping>()
            .ReverseMap();

        CreateMap<MenuProductMappingForKioskDto, MenuProductMapping>()
            .ReverseMap();

        CreateMap<UpdateMenuProductMappingDto, MenuProductMapping>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<CreateMenuProductMappingDto, MenuProductMapping>()
            .AfterMap((src, dest, context) =>
            {
                if (context.Items.TryGetValue("DisplayOrder", out var value) &&
                    int.TryParse(value?.ToString(), out var order))
                {
                    dest.DisplayOrder = order;
                }

                if (context.Items.TryGetValue("SellingPrice", out var sellingPrice) &&
                    decimal.TryParse(sellingPrice?.ToString(), out var price))
                {
                    dest.SellingPrice = price;
                }
            })
            .ReverseMap();
    }
}