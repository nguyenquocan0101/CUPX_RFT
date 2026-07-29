using AutoMapper;
using Domain.Models;
using Services.Dtos.Product;

namespace Kiosk.ApiService.Mappers
{
    public class ProductMapper : Profile
    {
        public ProductMapper()
        {
            CreateMap<Product, ProductDto>()
                .ReverseMap();
            CreateMap<UpdateProductDto, Product>()
               .ReverseMap();
            CreateMap<CreateProductDto, Product>()
              .ReverseMap();
        }
    }
}
