using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Newtonsoft.Json;
using Services.Dtos.OrderDetail;
using Services.Dtos.ProductAttribute;

namespace AutomaticBrewingCoffee.API.Mappers;

public class OrderDetailMapper : Profile
{
    public OrderDetailMapper()
    {
        CreateMap<OrderDetailNestedDto, OrderDetail>()
            .ForMember(dest => dest.OrderDetailId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.ProductAttributes,
                opt => opt.MapFrom(src => src.ProductAttributes != null
                    ? JsonConvert.SerializeObject(src.ProductAttributes)
                    : null))
            .ReverseMap()
            .ForMember(dest => dest.ProductAttributes,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ProductAttributes)
                    ? JsonConvert.DeserializeObject<List<ProductAttributeSelectDto>>(src.ProductAttributes!)
                    : new List<ProductAttributeSelectDto>()));


        CreateMap<OrderDetailDto, OrderDetail>()
            .ForMember(dest => dest.ProductAttributes,
                opt => opt.MapFrom(src => src.ProductAttributes != null
                    ? JsonConvert.SerializeObject(src.ProductAttributes)
                    : null))
            .ReverseMap()
            .ForMember(dest => dest.ProductAttributes,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ProductAttributes)
                    ? JsonConvert.DeserializeObject<List<ProductAttributeSelectDto>>(src.ProductAttributes!)
                    : new List<ProductAttributeSelectDto>()));
    }
}