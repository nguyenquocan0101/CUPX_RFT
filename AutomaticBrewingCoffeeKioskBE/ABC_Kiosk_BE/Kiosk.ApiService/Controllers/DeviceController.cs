using AutomaticBrewingCoffee.API.Constants;
using Domain.Models;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.DeviceParameter;
using Services.Interfaces;

namespace Kiosk.ApiService.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/devices")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        
        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDevices([FromQuery] DeviceQueryDto dto)
        {
            var result = await _deviceService.GetDevices(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{deviceId}/parameters")]
        [ProducesResponseType(typeof(BaseResult<string, DeviceParameterDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeviceParameters(string deviceId)
        {
            var result = await _deviceService.GetDeviceParameters(deviceId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{deviceId}/coordinates")]
        [ProducesResponseType(typeof(BaseResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateDeviceCoordinates(string deviceId, UpdateDeviceCoordinateDto request)
        {
            var result = await _deviceService.UpdateDeviceCoordinates(deviceId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("parameters")]
        [ProducesResponseType(typeof(BaseResult<SetDeviceParameterDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetDeviceParameters([FromQuery] SetDeviceParameterDto dto)
        {
            var result = await _deviceService.SetDeviceParameters(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDevice(CreateDeviceDto dto)
        {
            var result = await _deviceService.CreateDevice(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{deviceId}")]
        public async Task<IActionResult> PutDevice(string deviceId, UpdateDeviceDto dto)
        {
            var result = await _deviceService.UpdateDevice(deviceId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDevice(string deviceId)
        {
            var result = await _deviceService.RemoveDevice(deviceId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetDeviceById(string deviceId)
        {
            var result = await _deviceService.GetDeviceById(deviceId);
            return StatusCode(result.StatusCode, result);
        }

        //[HttpGet("monitor")]
        //public async Task<IActionResult> GetAllDeviceStatuses()
        //{
        //    var result = await _deviceService.GetAllDeviceStatusAsync();
        //    return StatusCode(result.StatusCode, result);
        //}
    }
}