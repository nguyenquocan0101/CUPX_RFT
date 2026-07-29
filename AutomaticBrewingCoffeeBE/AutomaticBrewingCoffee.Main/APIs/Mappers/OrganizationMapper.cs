using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.Organization;
using Services.Utils;

namespace AutomaticBrewingCoffee.API.Mappers;

public class OrganizationMapper : Profile
{
    public OrganizationMapper()
    {
        CreateMap<CreateOrganizationDto, OrganizationDto>()
            .ReverseMap();

        CreateMap<CreateOrganizationDto, Organization>()
            .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                var nameNoWhiteSpace = new string(dest.Name.Where(c => !char.IsWhiteSpace(c)).ToArray());
                var normalizedName = StringHelper.RemoveDiacritics(nameNoWhiteSpace);
                var shortOrgId = GuidUtil.ShortenGuid(Guid.Parse(dest.OrganizationId));

                var orgCode = $"ORG{normalizedName}{shortOrgId}";
                dest.OrganizationCode = orgCode;
            })
            .ReverseMap();

        CreateMap<UpdateOrganizationDto, Organization>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap();

        CreateMap<IPaginate<OrganizationDto>, IPaginate<Organization>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<Organization, OrganizationDto>()
            .ReverseMap();

        CreateMap<Organization, OrganizationReverseDto>()
            .ReverseMap();
    }
}