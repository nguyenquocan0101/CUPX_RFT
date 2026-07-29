using AutoMapper;
using Services.Dtos.Workflow;
using Domain.Models;

namespace AutomaticBrewingCoffee.API.Mappers;

public class WorkflowMapper : Profile
{
    public WorkflowMapper()
    {
        CreateMap<CreateWorkflowDto, WorkflowDto>()
            .ReverseMap();

        CreateMap<UpdateWorkflowDto, Workflow>()
            .ReverseMap();

        CreateMap<WorkflowDto, Workflow>()
            .ReverseMap();
    }
}