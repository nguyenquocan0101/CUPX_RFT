using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.DeviceModel;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/device-models")]
    [ApiController]
    [TrimStrings]
    public class DeviceModelsController : ControllerBase
    {
        private readonly IDeviceModelService _deviceModelService;

        public DeviceModelsController(IDeviceModelService deviceModelService)
        {
            _deviceModelService = deviceModelService;
        }

        // GET: api/device-models
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of device models",
            Description = "Retrieve a paginated list of device models with optional filters."
        )]
        public async Task<ActionResult<BaseResult<DeviceModelQueryDto, Paginate<DeviceModelDto>>>> Get(
            [FromQuery] DeviceModelQueryDto deviceModelQueryDto)
        {
            var response = await _deviceModelService.GetDeviceModels(deviceModelQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/device-models/{deviceModelId}
        [HttpGet("{deviceModelId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get device model details",
            Description = "Retrieve detailed information about a specific device model by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, DeviceModelDto>>> Get(string deviceModelId)
        {
            var response = await _deviceModelService.GetDeviceModel(deviceModelId);
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/device-models
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new device model",
            Description = "Create a new device model by providing necessary details."
        )]
        public async Task<ActionResult<BaseResult<CreateDeviceModelDto, DeviceModelDto>>> Post(
            [FromBody] CreateDeviceModelDto createDeviceModelDto)
        {
            var response = await _deviceModelService.CreateDeviceModel(createDeviceModelDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT: api/device-models/{deviceModelId}
        [HttpPut("{deviceModelId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update device model details",
            Description = "Update the details of an existing device model by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateDeviceModelDto, DeviceModelDto>>> Put(string deviceModelId,
            [FromBody] UpdateDeviceModelDto updateDeviceModelDto)
        {
            var response = await _deviceModelService.UpdateDeviceModel(deviceModelId, updateDeviceModelDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE: api/device-models/{deviceModelId}
        [HttpDelete("{deviceModelId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a device model",
            Description = "Delete an existing device model by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, DeviceModelDto>>> Delete(string deviceModelId)
        {
            var response = await _deviceModelService.RemoveDeviceModel(deviceModelId);
            return StatusCode(response.StatusCode, response);
        }
    }
}