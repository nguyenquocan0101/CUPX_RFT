using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Newtonsoft.Json;
using Services.Dtos.Step;
using Services.Utils;

namespace AutomaticBrewingCoffee.API.Mappers;

public class StepMapper : Profile
{
    public StepMapper()
    {
        CreateMap<StepNestedDto, Step>()
            .ForMember(dest => dest.StepId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src =>
                src.Conditions == null
                    ? null
                    : JsonConvert.SerializeObject(
                        src.Conditions.Select(c => new StepConditionRawDto
                        {
                            Name = c.Name,
                            Description = c.Description,
                            Expression = ExpressionHelper.ToExpressionString(c.Expression)
                        })
                    )
            ))
            .ReverseMap()
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Conditions)
                    ? null
                    : JsonConvert.DeserializeObject<List<StepConditionRawDto>>(src.Conditions)!.Select(c =>
                        new StepConditionDto
                        {
                            Name = c.Name,
                            Description = c.Description,
                            Expression = ExpressionHelper.ParseExpressionString(c.Expression)
                        }).ToList()
            ));


        CreateMap<StepInsideDto, Step>()
            .ForMember(dest => dest.StepId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ReverseMap()
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Conditions)
                    ? null
                    : JsonConvert.DeserializeObject<List<StepConditionRawDto>>(src.Conditions)!.Select(c =>
                        new StepConditionDto
                        {
                            Name = c.Name,
                            Description = c.Description,
                            Expression = ExpressionHelper.ParseExpressionString(c.Expression)
                        }).ToList()
            ));

        CreateMap<CreateStepDto, Step>()
            .ForMember(dest => dest.StepId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .AfterMap((src, dest, context) =>
            {
                // if (context.Items.TryGetValue("Sequence", out var value) &&
                //     int.TryParse(value?.ToString(), out var sequence))
                // {
                //     dest.Sequence = sequence;
                // }
            })
            .ReverseMap();

        CreateMap<Step, Step>()
            .ForMember(dest => dest.StepId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap()
            .ForMember(dest => dest.StepId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());
    }
}