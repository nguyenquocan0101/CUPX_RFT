using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.Kiosk;
using Services.Dtos.KioskDevice;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/kiosks")]
    [ApiController]
    [TrimStrings]
    public class KiosksController : ControllerBase
    {
        private readonly IKioskService _kioskService;

        public KiosksController(IKioskService kioskService)
        {
            _kioskService = kioskService;
        }

        // GET: api/<KiosksController>
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of kiosks",
            Description = "Retrieve a paginated list of kiosks with optional filters such as status or location."
        )]
        public async Task<ActionResult<BaseResult<KioskQueryDto, Paginate<KioskDto>>>> Get(
            [FromQuery] KioskQueryDto kioskQueryDto)
        {
            var response = await _kioskService.GetKiosks(kioskQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET api/<KiosksController>/5
        [HttpGet("{kioskId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get kiosk details",
            Description = "Retrieve detailed information about a specific kiosk by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDto>>> Get(string kioskId)
        {
            var response = await _kioskService.GetKiosk(kioskId);
            return StatusCode(response.StatusCode, response);
        }

        // POST api/<KiosksController>
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new kiosk",
            Description = "Create a new kiosk with details such as location, status, and other required data."
        )]
        public async Task<ActionResult<BaseResult<CreateKioskDto, KioskDto>>> Post(
            [FromBody] CreateKioskDto createKioskDto)
        {
            var response = await _kioskService.CreateKiosk(createKioskDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT api/<KiosksController>/5
        [HttpPut("{kioskId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update kiosk details",
            Description = "Update the details of an existing kiosk by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateKioskDto, KioskDto>>> Put(string kioskId,
            [FromBody] UpdateKioskDto updateKioskDto)
        {
            var response = await _kioskService.UpdateKiosk(kioskId, updateKioskDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE api/<KiosksController>/5
        [HttpDelete("{kioskId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a kiosk",
            Description = "Delete an existing kiosk by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDto>>> Delete(string kioskId)
        {
            var response = await _kioskService.RemoveKiosk(kioskId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("devices")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Add a device into a kiosk",
            Description = "Add an existing device by its kiosk Id."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDeviceDto>>> AddDevice(
            [FromBody] AddKioskDeviceDto addKioskDeviceDto
        )
        {
            var response = await _kioskService.AddKioskDevice(addKioskDeviceDto);
            return StatusCode(response.StatusCode, response);
        }

        // [HttpPut("devices/{kioskDeviceId}/status")]
        // [SwaggerOperation(
        //     Summary = "Change a device status of a kiosk",
        //     Description = "Change a device status of a kiosk by kiosk device id."
        // )]
        // public async Task<ActionResult<BaseResult<string, KioskDeviceDto>>> ChangeKioskDeviceStatus(
        //     [FromRoute] string kioskDeviceId,
        //     [FromBody] ChangeKioskDeviceStatusDto changeKioskDeviceStatusDto
        // )
        // {
        //     var response = await _kioskService.ChangeKioskDeviceStatus(kioskDeviceId, changeKioskDeviceStatusDto);
        //     return StatusCode(response.StatusCode, response);
        // }

        [HttpPut("devices/{kioskDeviceId}/dispose")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Dispose a device of a kiosk",
            Description = "Dispose a device of a kiosk by kiosk device id."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDeviceDto>>> ChangeKioskDeviceStatus(
            [FromRoute] string kioskDeviceId
        )
        {
            var response = await _kioskService.DisposeKioskDevice(kioskDeviceId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("devices/{kioskDeviceId}/replace")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Replace a device of a kiosk",
            Description = "Replace a device of a kiosk by kiosk device id."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDeviceDto>>> ReplaceDevice(
            [FromRoute] string kioskDeviceId,
            [FromBody] ReplaceDeviceDto replaceDeviceDto
        )
        {
            var response = await _kioskService.ReplaceDevice(kioskDeviceId, replaceDeviceDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("devices/{kioskDeviceId}/on-hub")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Replace a device of a kiosk",
            Description = "Replace a device of a kiosk by kiosk device id."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDeviceOnHubDto>>> GetKioskDeviceOnHub(
            string kioskDeviceId)
        {
            var response = await _kioskService.GetKioskDeviceOnHub(kioskDeviceId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{kioskId}/export-setup")]
        [Authorizes(nameof(ERoleName.Admin))]
        public async Task<ActionResult> ExportSetup(string kioskId)
        {
            var response = await _kioskService.ExportKioskSetup(kioskId);

            if (response is null)
            {
                return NotFound();
            }

            return File(response, "application/zip", "Kiosk.zip");
        }

        [HttpPost("assign-menu")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        public async Task<ActionResult<BaseResult<AssignKioskMenuDto, KioskDto>>> AssignKioskMenu(
            AssignKioskMenuDto assignMenuDto)
        {
            var response = await _kioskService.AssignKioskMenu(assignMenuDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{kioskId}/devices/on-place")]
        [Authorizes(nameof(ERoleName.Admin))]
        public async Task<ActionResult<BaseResult<KioskDeviceOnPlaceQueryDto, KioskDeviceOnPlaceDto>>>
            GetKioskDeviceOnPlace(
                [FromRoute] string kioskId,
                [FromQuery] KioskDeviceOnPlaceQueryDto kioskDeviceOnPlaceQueryDto
            )
        {
            var response = await _kioskService.GetKioskDeviceOnPlace(kioskId, kioskDeviceOnPlaceQueryDto);

            return StatusCode(response.StatusCode, response);
        }

        // GET api/<KiosksController>/5
        [HttpGet("current")]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Get current kiosk details for tablet",
            Description = "Retrieve detailed information about a specific kiosk by its ID for tablet."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDto>>> Get()
        {
            var response = await _kioskService.GetCurrentKiosk();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("clean")]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Clean current kiosk for tablet",
            Description = "Execute the clean workflow."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDto>>> Clean()
        {
            var response = await _kioskService.Clean();
            return StatusCode(response.StatusCode, response);
        }
        
        [HttpPost("ping")]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Clean current kiosk for tablet",
            Description = "Execute the clean workflow."
        )]
        public async Task<ActionResult<BaseResult<string, KioskDto>>> Ping()
        {
            var response = await _kioskService.Ping();
            return StatusCode(response.StatusCode, response);
        }
    }
}