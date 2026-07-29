using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.ProductCategory;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/product-categories")]
    [ApiController]
    [TrimStrings]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoryController(IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of product categories",
            Description = "Retrieve a paginated list of product categories with optional filters like name or status."
        )]
        public async Task<ActionResult<BaseResult<ProductCategoryQueryDto, Paginate<ProductCategoryDto>>>> Get(
            [FromQuery] ProductCategoryQueryDto queryDto)
        {
            var response = await _productCategoryService.GetProductCategories(queryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{productCategoryId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get product category details",
            Description = "Retrieve detailed information about a specific product category by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, ProductCategoryDto>>> Get(string productCategoryId)
        {
            var response = await _productCategoryService.GetProductCategory(productCategoryId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new product category",
            Description = "Create a new product category with name, image, and status."
        )]
        public async Task<ActionResult<BaseResult<CreateProductCategoryDto, ProductCategoryDto>>> Post(
            [FromBody] CreateProductCategoryDto createDto)
        {
            var response = await _productCategoryService.CreateProductCategory(createDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{productCategoryId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update a product category",
            Description = "Update an existing product category by ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateProductCategoryDto, ProductCategoryDto>>> Put(
            string productCategoryId,
            [FromBody] UpdateProductCategoryDto updateDto)
        {
            var response = await _productCategoryService.UpdateProductCategory(productCategoryId, updateDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{productCategoryId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a product category",
            Description =
                "Delete a product category by ID. If the category is used by any product, deletion is blocked."
        )]
        public async Task<ActionResult<BaseResult<string, ProductCategoryDto>>> Delete(string productCategoryId)
        {
            var response = await _productCategoryService.RemoveProductCategory(productCategoryId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("reorder")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Reorder a category by its id",
            Description = "Reorder a a category by its id"
        )]
        public async Task<ActionResult<BaseResult<ReorderProductCategoryDto, ProductCategoryDto>>> ReorderStep(
            ReorderProductCategoryDto reorderProductCategoryDto)
        {
            var response = await _productCategoryService.ReorderProductCategory(reorderProductCategoryDto);
            return StatusCode(response.StatusCode, response);
        }
    }
}