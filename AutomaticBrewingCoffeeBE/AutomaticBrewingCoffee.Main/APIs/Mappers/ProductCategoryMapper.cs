using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.ProductCategory;

namespace AutomaticBrewingCoffee.API.Mappers;

public class ProductCategoryMapper : Profile
{
    public ProductCategoryMapper()
    {
        CreateMap<CreateProductCategoryDto, ProductCategoryDto>()
            .ReverseMap();

        CreateMap<CreateProductCategoryDto, ProductCategory>()
            .ForMember(dest => dest.ProductCategoryId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<UpdateProductCategoryDto, ProductCategory>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<ProductCategoryDto>, IPaginate<ProductCategory>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<ProductCategory, ProductCategoryDto>()
            .ReverseMap();

        CreateMap<ProductCategory, ProductCategoryInsideDto>()
            .ReverseMap();
    }
}