using AutomaticBrewingCoffee.API.Constants;
using Kiosk.ApiService.Filters;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.KioskMachine;
using Services.Interfaces;

namespace Kiosk.ApiService.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}")]
    [ApiController]
    public class KioskMachineController : ControllerBase
    {
        private readonly IWorkflowService2 _workflowService2;
        private readonly IDeviceService2 _deviceService;
        private readonly IOrderCacheService _orderCacheService;
        private readonly IRuntimeStateService _runtimeStateService;


        public KioskMachineController(IWorkflowService2 workflowService2, IOrderCacheService orderCacheService,
            IDeviceService2 deviceService, IRuntimeStateService runtimeStateService)
        {
            _workflowService2 = workflowService2;
            _deviceService = deviceService;
            _orderCacheService = orderCacheService;
            _runtimeStateService = runtimeStateService;
        }

        [HttpGet("ping")]
        [ServiceFilter(typeof(MaintenanceFilter))]
        public async Task<IActionResult> Ping()
        {
            return await Task.FromResult(Ok(new BaseResult(StatusCodes.Status200OK, "ok", true)));
        }

        [HttpPost("execute")]
        [ServiceFilter(typeof(MaintenanceFilter))]
        public async Task<IActionResult> ExecuteWorkflowAsync(ExecuteWorkflowDto dto)
        {
            try
            {
                var productIdList = dto.WorkflowIds.Select(x => x.WorkflowId).ToList();
                await _orderCacheService.AddAsync(dto.OrderId, productIdList);
                var result = await _workflowService2.ExecuteWorkflowAsync(dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception)
            {
                //incase exception - just remove order in redis for not having unexpected consequence 
                await _orderCacheService.RemoveOrder(dto.OrderId);
                throw;
            }
        }
        [HttpGet("workflows")]
        public async Task<IActionResult> GetAllWorkflowsAsync()
        {

            var result = await _workflowService2.GetAllWorkflowsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("clean-workflows")]
        public async Task<IActionResult> GetAllCleanWorkflowsAsync()
        {

            var result = await _workflowService2.GetAllCleanWorkflowsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("execute-clean")]
        [ServiceFilter(typeof(MaintenanceFilter))]
        public async Task<IActionResult> ExecuteCleanWorkflowAsync(ExecuteCleanWorkflowDto dto)
        {
            try
            {
                var result = await _workflowService2.ExecuteCleanWorkflowAsync(dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception)
            {
                await _runtimeStateService.SetMaintenanceAsync(false);
                throw;
            }
        }

        [HttpGet("doc/devices")]
        public async Task<IActionResult> GetDevicesAsync([FromQuery] DeviceDocQueryDto query)
        {
            var result = await _deviceService.GetAllDeviceDocsAsync(query);
            return StatusCode(result.StatusCode, result);
        }

    }
}
