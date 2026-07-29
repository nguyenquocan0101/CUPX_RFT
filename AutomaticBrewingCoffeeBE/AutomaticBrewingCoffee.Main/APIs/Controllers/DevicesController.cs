using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.DeviceIngredientState;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/devices")]
    [ApiController]
    [TrimStrings]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }


        // GET: api/<DevicesController>
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of devices",
            Description = "Retrieve a paginated list of devices with optional filters like type, status, etc."
        )]
        public async Task<ActionResult<BaseResult<DeviceQueryDto, Paginate<DeviceDto>>>> Get(
            [FromQuery] DeviceQueryDto deviceQueryDto)
        {
            var response = await _deviceService.GetDevices(deviceQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET api/<DevicesController>/5
        [HttpGet("{deviceId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get device details",
            Description = "Retrieve detailed information about a specific device by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, DeviceDto>>> Get(string deviceId)
        {
            var response = await _deviceService.GetDevice(deviceId);
            return StatusCode(response.StatusCode, response);
        }

        // POST api/<DevicesController>
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new device",
            Description = "Create a new device by providing necessary details such as type, model, etc."
        )]
        public async Task<ActionResult<BaseResult<CreateDeviceDto, DeviceDto>>> Post(
            [FromBody] CreateDeviceDto createDeviceDto)
        {
            var response = await _deviceService.CreateDevice(createDeviceDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/<DevicesController>/5
        [HttpPut("{deviceId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update device details",
            Description = "Update the details of an existing device by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateDeviceDto, DeviceDto>>> Put(string deviceId,
            [FromBody] UpdateDeviceDto updateDeviceDto)
        {
            var response = await _deviceService.UpdateDevice(deviceId, updateDeviceDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE api/<DevicesController>/5
        [HttpDelete("{deviceId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a device",
            Description = "Delete an existing device by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, DeviceDto>>> Delete(string deviceId)
        {
            var response = await _deviceService.RemoveDevice(deviceId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{deviceId}/replace")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get all device similar to device replace",
            Description = "This api will get all device similar to device replace by the deviceModelId."
        )]
        public async Task<ActionResult<BaseResult<string, Paginate<DeviceDto>>>> GetReplaceDevice(
            [FromRoute] string deviceId,
            [FromQuery] DeviceQueryDto deviceQueryDto
        )
        {
            var response = await _deviceService.GetDeviceReplace(deviceId, deviceQueryDto);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPut("ingredient/{ingredientStateId}/by-kiosk")]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Update device ingredient for device",
            Description = "Update device ingredient for device by its id."
        )]
        public async Task<ActionResult<BaseResult<string, DeviceDto>>> UpdateKioskDeviceIngredient(
            [FromRoute] string ingredientStateId,
            [FromBody] UpdateDeviceIngredientStateDto updateDeviceIngredientStateDto
        )
        {
            var response =
                await _deviceService.UpdateDeviceIngredientState(ingredientStateId, updateDeviceIngredientStateDto);
            return StatusCode(response.StatusCode, response);
        }
    }
}