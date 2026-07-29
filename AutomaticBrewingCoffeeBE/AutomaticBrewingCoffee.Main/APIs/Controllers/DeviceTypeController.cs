using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.DeviceType;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/device-types")]
    [ApiController]
    [TrimStrings]
    public class DeviceTypesController : ControllerBase
    {
        private readonly IDeviceTypeService _deviceTypeService;

        public DeviceTypesController(IDeviceTypeService deviceTypeService)
        {
            _deviceTypeService = deviceTypeService;
        }

        // GET: api/device-types
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of device types",
            Description = "Retrieve a paginated list of device types with optional filters."
        )]
        public async Task<ActionResult<BaseResult<DeviceTypeQueryDto, Paginate<DeviceTypeDto>>>> Get(
            [FromQuery] DeviceTypeQueryDto deviceTypeQueryDto)
        {
            var response = await _deviceTypeService.GetDeviceTypes(deviceTypeQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/device-types/{deviceTypeId}
        [HttpGet("{deviceTypeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get device type details",
            Description = "Retrieve detailed information about a specific device type by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, DeviceTypeDto>>> Get(string deviceTypeId)
        {
            var response = await _deviceTypeService.GetDeviceType(deviceTypeId);
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/device-types
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new device type",
            Description = "Create a new device type by providing necessary details."
        )]
        public async Task<ActionResult<BaseResult<CreateDeviceTypeDto, DeviceTypeDto>>> Post(
            [FromBody] CreateDeviceTypeDto createDeviceTypeDto)
        {
            var response = await _deviceTypeService.CreateDeviceType(createDeviceTypeDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT: api/device-types/{deviceTypeId}
        [HttpPut("{deviceTypeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update device type details",
            Description = "Update the details of an existing device type by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateDeviceTypeDto, DeviceTypeDto>>> Put(string deviceTypeId,
            [FromBody] UpdateDeviceTypeDto updateDeviceTypeDto)
        {
            var response = await _deviceTypeService.UpdateDeviceType(deviceTypeId, updateDeviceTypeDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE: api/device-types/{deviceTypeId}
        [HttpDelete("{deviceTypeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a device type",
            Description = "Delete an existing device type by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, DeviceTypeDto>>> Delete(string deviceTypeId)
        {
            var response = await _deviceTypeService.RemoveDeviceType(deviceTypeId);
            return StatusCode(response.StatusCode, response);
        }
    }
}