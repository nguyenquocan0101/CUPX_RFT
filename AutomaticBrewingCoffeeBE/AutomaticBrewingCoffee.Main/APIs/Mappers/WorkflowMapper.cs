using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Step;
using Services.Dtos.Workflow;

namespace AutomaticBrewingCoffee.API.Mappers;

public class WorkflowMapper : Profile
{
    public WorkflowMapper()
    {
        CreateMap<CreateWorkflowDto, Workflow>()
            .ForMember(dest => dest.WorkflowId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .AfterMap((src, dest) =>
            {
                if (dest.Steps is null)
                {
                    return;
                }

                foreach (var step in dest.Steps)
                {
                    step.WorkflowId = dest.WorkflowId;
                }
            })
            .ReverseMap();

        CreateMap<WorkflowDto, Workflow>()
            .ReverseMap();

        CreateMap<UpdateWorkflowDto, Workflow>()
            .ReverseMap();

        CreateMap<Workflow, Workflow>()
            .ForMember(dest => dest.WorkflowId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap()
            .ForMember(dest => dest.WorkflowId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());
    }
}