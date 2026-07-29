using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.ProductAttribute;

namespace AutomaticBrewingCoffee.API.Mappers;

public class ProductAttributeMapper : Profile
{
    public ProductAttributeMapper()
    {
        CreateMap<ProductAttributeInsideDto, ProductAttribute>()
            .ReverseMap();

        CreateMap<ProductAttributeNestedDto, ProductAttribute>()
            .ForMember(dest => dest.ProductAttributeId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                var attributeOptions = dest.AttributeOptions;

                if (attributeOptions == null)
                {
                    return;
                }

                var productAttributeId = dest.ProductAttributeId;

                foreach (var attributeOption in attributeOptions)
                {
                    attributeOption.ProductAttributeId = productAttributeId;
                }
            })
            .ReverseMap();

        CreateMap<ProductAttribute, ProductAttribute>()
            .ForMember(dest => dest.ProductAttributeId,
                opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap()
            .ForMember(dest => dest.ProductAttributeId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());
    }
}