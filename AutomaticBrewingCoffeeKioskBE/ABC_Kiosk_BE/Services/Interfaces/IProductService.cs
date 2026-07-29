using Domain.Pagination;
using Services.Base;
using Services.Dtos.Product;

namespace Services.Interfaces
{
    public interface IProductService
    {
        Task<BaseResult<ProductQueryDto, Paginate<ProductDto>>> GetProducts(ProductQueryDto productQueryDto);
        Task<BaseResult<string, ProductDto>> GetProduct(string productId);
        Task<BaseResult<CreateProductDto, ProductDto>> CreateProduct(CreateProductDto createProductDto);
        Task<BaseResult<UpdateProductDto, ProductDto>> UpdateProduct(string productId, UpdateProductDto updateProductDto);
        Task<BaseResult<string, ProductDto>> RemoveProduct(string productId);
    }
}
