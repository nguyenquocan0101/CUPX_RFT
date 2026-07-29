using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Organization;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/organizations")]
    [ApiController]
    [TrimStrings]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        // GET: api/organizations
        [HttpGet]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get list of organizations",
            Description = "Retrieve a paginated list of organizations with optional filters."
        )]
        public async Task<ActionResult<BaseResult<OrganizationQueryDto, Paginate<OrganizationDto>>>> Get(
            [FromQuery] OrganizationQueryDto organizationQueryDto)
        {
            var response = await _organizationService.GetOrganizations(organizationQueryDto);
            return StatusCode(response.StatusCode, response);
        }

        // GET: api/organizations/{organizationId}
        [HttpGet("{organizationId}")]
        [Authorizes(nameof(ERoleName.Admin), nameof(ERoleName.Organization))]
        [SwaggerOperation(
            Summary = "Get organization details",
            Description = "Retrieve detailed information about a specific organization by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, OrganizationDto>>> Get(string organizationId)
        {
            var response = await _organizationService.GetOrganization(organizationId);
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/organizations
        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Create a new organization",
            Description = "Create a new organization by providing necessary details."
        )]
        public async Task<ActionResult<BaseResult<CreateOrganizationDto, OrganizationDto>>> Post(
            [FromBody] CreateOrganizationDto createOrganizationDto)
        {
            var response = await _organizationService.CreateOrganization(createOrganizationDto);
            return StatusCode(response.StatusCode, response);
        }

        // PUT: api/organizations/{organizationId}
        [HttpPut("{organizationId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update organization details",
            Description = "Update the details of an existing organization by its ID."
        )]
        public async Task<ActionResult<BaseResult<UpdateOrganizationDto, OrganizationDto>>> Put(
            string organizationId,
            [FromBody] UpdateOrganizationDto updateOrganizationDto)
        {
            var response = await _organizationService.UpdateOrganization(organizationId, updateOrganizationDto);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE: api/organizations/{organizationId}
        [HttpDelete("{organizationId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Delete an organization",
            Description = "Delete an existing organization by its ID."
        )]
        public async Task<ActionResult<BaseResult<string, OrganizationDto>>> Delete(string organizationId)
        {
            var response = await _organizationService.RemoveOrganization(organizationId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("current")]
        [ApiKeyAuth]
        [SwaggerOperation(
            Summary = "Get organization details for tablet",
            Description = "Retrieve detailed information about a specific organization by its ID for tablet."
        )]
        public async Task<ActionResult<BaseResult<string, OrganizationDto>>> Get()
        {
            var response = await _organizationService.GetCurrentOrganization();
            return StatusCode(response.StatusCode, response);
        }
    }
}