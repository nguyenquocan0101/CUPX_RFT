using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Store;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/stores")]
    [ApiController]
    [TrimStrings]
    public class StoresController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of stores",
            Description = "Retrieve a paginated list of stores with optional filters."
        )]
        public async Task<ActionResult<BaseResult<StoreQueryDto, Paginate<StoreDto>>>> Get(
            [FromQuery] StoreQueryDto storeQueryDto)
        {
            var response = await _storeService.GetStores(storeQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{storeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get store details",
            Description = "Retrieve detailed information about a specific store by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, StoreDto>>> Get(string storeId)
        {
            var response = await _storeService.GetStore(storeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new store",
            Description = "Create a new store by providing necessary details."
        )]
        public async Task<ActionResult<BaseResult<CreateStoreDto, StoreDto>>> Post(
            [FromBody] CreateStoreDto createStoreDto)
        {
            var response = await _storeService.CreateStore(createStoreDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{storeId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Update store details",
            Description = "Update the details of an existing store by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateStoreDto, StoreDto>>> Put(string storeId,
            [FromBody] UpdateStoreDto updateStoreDto)
        {
            var response = await _storeService.UpdateStore(storeId, updateStoreDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{storeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a store",
            Description = "Delete an existing store by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, StoreDto>>> Delete(string storeId)
        {
            var response = await _storeService.RemoveStore(storeId);
            return StatusCode(response.StatusCode, response);
        }
    }
}