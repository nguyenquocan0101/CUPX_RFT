using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.Product;

namespace AutomaticBrewingCoffee.API.Mappers;

public class ProductMapper : Profile
{
    public ProductMapper()
    {
        CreateMap<CreateProductDto, ProductDto>()
            .ReverseMap();

        CreateMap<ProductNestedDto, Product>()
            .ReverseMap();

        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .AfterMap((src, dest) =>
            {
                var productAttributes = dest.ProductAttributes;

                if (productAttributes == null)
                {
                    return;
                }

                var productId = dest.ProductId;

                foreach (var productAttribute in productAttributes)
                {
                    productAttribute.ProductId = productId;
                }
            })
            .ReverseMap();

        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<ProductDto>, IPaginate<Product>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<ProductDto, Product>()
            .ReverseMap()
            .ForMember(dest => dest.ProductParentName, opt => opt.MapFrom(src => src.Parent.Name));

        CreateMap<ProductForKioskDto, Product>()
            .ReverseMap()
            .ForMember(dest => dest.ProductParentName, opt => opt.MapFrom(src => src.Parent.Name));

        CreateMap<Product, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .AfterMap((src, dest, context) =>
            {
                if (dest.Workflows is null)
                {
                    return;
                }

                foreach (var workflow in dest.Workflows)
                {
                    workflow.ProductId = dest.ProductId;

                    if (workflow.Steps is null)
                    {
                        continue;
                    }

                    foreach (var step in workflow.Steps)
                    {
                        step.WorkflowId = workflow.WorkflowId;
                    }
                }

                if (dest.ProductAttributes is null)
                {
                    return;
                }

                foreach (var productAttribute in dest.ProductAttributes)
                {
                    productAttribute.ProductId = dest.ProductId;

                    if (productAttribute.AttributeOptions is null)
                    {
                        continue;
                    }

                    foreach (var attributeOption in productAttribute.AttributeOptions)
                    {
                        attributeOption.ProductAttributeId = productAttribute.ProductAttributeId;
                    }
                }
            });
    }
}