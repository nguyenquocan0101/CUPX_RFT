using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.LocationType;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/location-types")]
    [ApiController]
    [TrimStrings]
    public class LocationTypesController : ControllerBase
    {
        private readonly ILocationTypeService _locationTypeService;

        public LocationTypesController(ILocationTypeService locationTypeService)
        {
            _locationTypeService = locationTypeService;
        }

        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of location types",
            Description = "Retrieve a paginated list of location types with optional filters."
        )]
        public async Task<ActionResult<BaseResult<LocationTypeQueryDto, Paginate<LocationTypeDto>>>> Get(
            [FromQuery] LocationTypeQueryDto locationTypeQueryDto)
        {
            var response = await _locationTypeService.GetLocationTypes(locationTypeQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{locationTypeId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get location type by ID",
            Description = "Retrieve details of a specific location type using its ID."
        )]
        public async Task<ActionResult<BaseResult<string, LocationTypeDto>>> Get(string locationTypeId)
        {
            var response = await _locationTypeService.GetLocationType(locationTypeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create new location type",
            Description = "Create a new location type by providing necessary information."
        )]
        public async Task<ActionResult<BaseResult<CreateLocationTypeDto, LocationTypeDto>>> Post(
            [FromBody] CreateLocationTypeDto createLocationTypeDto)
        {
            var response = await _locationTypeService.CreateLocationType(createLocationTypeDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{locationTypeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update location type",
            Description = "Update an existing location type by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateLocationTypeDto, LocationTypeDto>>> Put(
            string locationTypeId,
            [FromBody] UpdateLocationTypeDto updateLocationTypeDto)
        {
            var response = await _locationTypeService.UpdateLocationType(locationTypeId, updateLocationTypeDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{locationTypeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete location type",
            Description = "Soft delete a location type by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, LocationTypeDto>>> Delete(string locationTypeId)
        {
            var response = await _locationTypeService.RemoveLocationType(locationTypeId);
            return StatusCode(response.StatusCode, response);
        }
    }
}