using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.KioskType;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/kiosk-types")]
    [ApiController]
    [TrimStrings]
    public class KioskTypesController : ControllerBase
    {
        private readonly IKioskTypeService _kioskTypeService;

        public KioskTypesController(IKioskTypeService kioskTypeService)
        {
            _kioskTypeService = kioskTypeService;
        }

        // GET: api/kiosk-types
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of kiosk types",
            Description = "Retrieve a paginated list of kiosk types with optional filters."
        )]
        public async Task<ActionResult<BaseResult<KioskTypeQueryDto, Paginate<KioskTypeDto>>>> Get(
            [FromQuery] KioskTypeQueryDto kioskTypeQueryDto)
        {
            var response = await _kioskTypeService.GetKioskTypes(kioskTypeQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/kiosk-types/{kioskTypeId}
        [HttpGet("{kioskTypeId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get kiosk type details",
            Description = "Retrieve detailed information about a specific kiosk type by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, KioskTypeDto>>> Get(string kioskTypeId)
        {
            var response = await _kioskTypeService.GetKioskType(kioskTypeId);
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/kiosk-types
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new kiosk type",
            Description = "Create a new kiosk type by providing necessary details."
        )]
        public async Task<ActionResult<BaseResult<CreateKioskTypeDto, KioskTypeDto>>> Post(
            [FromBody] CreateKioskTypeDto createKioskTypeDto)
        {
            var response = await _kioskTypeService.CreateKioskType(createKioskTypeDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT: api/kiosk-types/{kioskTypeId}
        [HttpPut("{kioskTypeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update kiosk type details",
            Description = "Update the details of an existing kiosk type by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateKioskTypeDto, KioskTypeDto>>> Put(string kioskTypeId,
            [FromBody] UpdateKioskTypeDto updateKioskTypeDto)
        {
            var response = await _kioskTypeService.UpdateKioskType(kioskTypeId, updateKioskTypeDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE: api/kiosk-types/{kioskTypeId}
        [HttpDelete("{kioskTypeId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete a kiosk type",
            Description = "Delete an existing kiosk type by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, KioskTypeDto>>> Delete(string kioskTypeId)
        {
            var response = await _kioskTypeService.RemoveKioskType(kioskTypeId);
            return StatusCode(response.StatusCode, response);
        }
    }
}