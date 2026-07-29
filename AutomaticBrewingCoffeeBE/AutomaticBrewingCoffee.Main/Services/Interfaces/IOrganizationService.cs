using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Organization;

namespace Services.Interfaces;

public interface IOrganizationService
{
    Task<BaseResult<OrganizationQueryDto, Paginate<OrganizationDto>>> GetOrganizations(
        OrganizationQueryDto organizationQueryDto);

    Task<BaseResult<string, OrganizationDto>> GetOrganization(string organizationId);

    Task<BaseResult<string, OrganizationReverseDto>> GetCurrentOrganization();

    Task<BaseResult<CreateOrganizationDto, OrganizationDto>> CreateOrganization(
        CreateOrganizationDto createOrganizationDto);

    Task<BaseResult<UpdateOrganizationDto, OrganizationDto>> UpdateOrganization(string organizationId,
        UpdateOrganizationDto updateOrganizationDto);

    Task<BaseResult<string, OrganizationDto>> RemoveOrganization(string organizationId);
}