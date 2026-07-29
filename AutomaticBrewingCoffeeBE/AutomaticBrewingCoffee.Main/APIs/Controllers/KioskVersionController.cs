using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.KioskVersion;
using Services.Dtos.KioskVersionDeviceModel;
using Services.Dtos.KioskVersionProduct;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/kiosk-versions")]
    [ApiController]
    [TrimStrings]
    public class KioskVersionsController : ControllerBase
    {
        private readonly IKioskVersionService _kioskVersionService;

        public KioskVersionsController(IKioskVersionService kioskVersionService)
        {
            _kioskVersionService = kioskVersionService;
        }

        // GET: api/kiosk-versions
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of kiosk versions",
            Description = "Retrieve a paginated list of kiosk versions with optional filters."
        )]
        public async Task<ActionResult<BaseResult<KioskVersionQueryDto, Paginate<KioskVersionDto>>>> Get(
            [FromQuery] KioskVersionQueryDto kioskVersionQueryDto)
        {
            var response = await _kioskVersionService.GetKioskVersions(kioskVersionQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/kiosk-versions/{kioskVersionId}
        [HttpGet("{kioskVersionId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get kiosk version details",
            Description = "Retrieve detailed information about a specific kiosk version by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, KioskVersionDto>>> Get(string kioskVersionId)
        {
            var response = await _kioskVersionService.GetKioskVersion(kioskVersionId);
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/kiosk-versions
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new kiosk version",
            Description = "Create a new kiosk version by providing necessary details."
        )]
        public async Task<ActionResult<BaseResult<CreateKioskVersionDto, KioskVersionDto>>> Post(
            [FromBody] CreateKioskVersionDto createKioskVersionDto)
        {
            var response = await _kioskVersionService.CreateKioskVersion(createKioskVersionDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT: api/kiosk-versions/{kioskVersionId}
        [HttpPut("{kioskVersionId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update kiosk version details",
            Description = "Update the details of an existing kiosk version by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateKioskVersionDto, KioskVersionDto>>> Put(
            string kioskVersionId,
            [FromBody] UpdateKioskVersionDto updateKioskVersionDto)
        {
            var response = await _kioskVersionService.UpdateKioskVersion(kioskVersionId, updateKioskVersionDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE: api/kiosk-versions/{kioskVersionId}
        [HttpDelete("{kioskVersionId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a kiosk version",
            Description = "Delete an existing kiosk version by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, KioskVersionDto>>> Delete(string kioskVersionId)
        {
            var response = await _kioskVersionService.RemoveKioskVersion(kioskVersionId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("device-models")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Add device model to kiosk version",
            Description = "Link a DeviceModel to a specific KioskVersion using their IDs."
        )]
        public async Task<ActionResult<BaseResult<AddKioskVersionDeviceModelDto, KioskVersionDeviceModelDto>>>
            AddDeviceModelToKioskVersion(
                [FromBody] AddKioskVersionDeviceModelDto addKioskDeviceDto
            )
        {
            var result = await _kioskVersionService.AddKioskVersionDeviceModel(addKioskDeviceDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{kioskVersionId}/device-models")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of Device Models mapped to a Kiosk Version.",
            Description = "Get list of Device Models mapped to a Kiosk Version."
        )]
        public async Task<ActionResult<BaseResult<string, Paginate<KioskVersionDeviceModelDto>>>>
            GetKioskVersionDeviceModels(
                [FromRoute] string kioskVersionId,
                [FromQuery] KioskVersionDeviceModelQueryDto kioskVersionDeviceModelQueryDto)
        {
            var result =
                await _kioskVersionService.GetKioskVersionDeviceModels(kioskVersionId, kioskVersionDeviceModelQueryDto);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("support-products")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Add support product to kiosk version",
            Description = "Link a support product to a specific KioskVersion using their IDs."
        )]
        public async Task<ActionResult<BaseResult<AddKioskVersionProductDto, KioskVersionProductDto>>>
            AddProductToKioskVersion(
                [FromBody] AddKioskVersionProductDto addKioskProductDto
            )
        {
            var result = await _kioskVersionService.AddKioskVersionProduct(addKioskProductDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{kioskVersionId}/support-products")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of Support Products mapped to a Kiosk Version.",
            Description = "Get list of Support Products mapped to a Kiosk Version."
        )]
        public async Task<ActionResult<BaseResult<string, Paginate<KioskVersionProductDto>>>> GetKioskVersionProducts(
            [FromRoute] string kioskVersionId,
            [FromQuery] KioskVersionProductQueryDto kioskVersionProductQueryDto
        )
        {
            var result =
                await _kioskVersionService.GetKioskVersionProduct(kioskVersionId, kioskVersionProductQueryDto);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{kioskVersionId}/valid-devices")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Get list of Support Device mapped to a Kiosk Version.",
            Description = "Get list of Support Device mapped to a Kiosk Version."
        )]
        public async Task<ActionResult<BaseResult<string, Paginate<KioskVersionProductDto>>>>
            GetKioskVersionValidDevice(
                [FromRoute] string kioskVersionId,
                [FromQuery] DeviceQueryDto deviceQueryDto
            )
        {
            var result =
                await _kioskVersionService.GetValidDevices(kioskVersionId, deviceQueryDto);

            return StatusCode(result.StatusCode, result);
        }
    }
}