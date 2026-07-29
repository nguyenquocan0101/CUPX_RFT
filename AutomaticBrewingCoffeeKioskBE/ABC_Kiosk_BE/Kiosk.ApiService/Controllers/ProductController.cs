using AutomaticBrewingCoffee.API.Constants;
using Microsoft.AspNetCore.Mvc;
using Services.Dtos.Product;
using Services.Interfaces;

namespace Kiosk.ApiService.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryDto dto)
        {
            var result = await _productService.GetProducts(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProducts(string productId)
        {
            var result = await _productService.GetProduct(productId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            var result = await _productService.CreateProduct(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(string productId, UpdateProductDto dto)
        {
            var result = await _productService.UpdateProduct(productId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            var result = await _productService.RemoveProduct(productId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
