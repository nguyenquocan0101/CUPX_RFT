using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Product;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/products")]
    [ApiController]
    [TrimStrings]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of products",
            Description = "Retrieve a paginated list of products with optional filters like category or availability."
        )]
        public async Task<ActionResult<BaseResult<ProductQueryDto, Paginate<ProductDto>>>> Get(
            [FromQuery] ProductQueryDto productQueryDto)
        {
            var response = await _productService.GetProducts(productQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{productId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get product details",
            Description = "Retrieve detailed information about a specific product by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, ProductDto>>> Get(string productId)
        {
            var response = await _productService.GetProduct(productId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{productId}/by-kiosk")]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Get product details",
            Description = "Retrieve detailed information about a specific product by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, ProductDto>>> GetByKiosk(string productId)
        {
            var response = await _productService.GetProduct(productId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new product",
            Description = "Create a new product with the provided information such as name, price, category, etc."
        )]
        public async Task<ActionResult<BaseResult<CreateProductDto, ProductDto>>> Post(
            [FromBody] CreateProductDto createProductDto)
        {
            var response = await _productService.CreateProduct(createProductDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("clone")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Clone a new product",
            Description = "Clone a new product with the provided information ids."
        )]
        public async Task<ActionResult<BaseResult<CreateProductDto, ProductDto>>> CloneProduct(
            [FromBody] CloneProductDto cloneProductDto)
        {
            var response = await _productService.CloneProduct(cloneProductDto.ProductId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{productId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update product details",
            Description = "Update the details of an existing product by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateProductDto, ProductDto>>> Put(string productId,
            [FromBody] UpdateProductDto updateProductDto)
        {
            var response = await _productService.UpdateProduct(productId, updateProductDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{productId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a product",
            Description = "Delete an existing product by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, ProductDto>>> Delete(string productId)
        {
            var response = await _productService.RemoveProduct(productId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{productId}/image")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Upload product image",
            Description = "Upload an image for a specific product by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, ProductDto>>> Delete(string productId,
            UploadProductImageDto uploadProductImageDto)
        {
            var response = await _productService.UploadImage(productId, uploadProductImageDto);
            return StatusCode(response.StatusCode, response);
        }
    }
}