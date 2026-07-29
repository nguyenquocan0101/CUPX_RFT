using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.ProductCategory;

namespace Services.Interfaces;

public interface IProductCategoryService
{
    Task<BaseResult<ProductCategoryQueryDto, Paginate<ProductCategoryDto>>> GetProductCategories(
        ProductCategoryQueryDto productCategoryQueryDto);

    Task<BaseResult<string, ProductCategoryDto>> GetProductCategory(string productCategoryId);

    Task<BaseResult<CreateProductCategoryDto, ProductCategoryDto>> CreateProductCategory(
        CreateProductCategoryDto createProductCategoryDto);

    Task<BaseResult<UpdateProductCategoryDto, ProductCategoryDto>> UpdateProductCategory(string productCategoryId,
        UpdateProductCategoryDto updateProductCategoryDto);

    Task<BaseResult<string, ProductCategoryDto>> RemoveProductCategory(string productCategoryId);
    
    Task<BaseResult<ReorderProductCategoryDto, List<ProductCategoryDto>>> ReorderProductCategory(ReorderProductCategoryDto reorderProductCategoryDto);
}