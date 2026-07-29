using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Newtonsoft.Json;
using Services.Dtos.FunctionParameter;

namespace AutomaticBrewingCoffee.API.Mappers;

public class FunctionParameterMapper : Profile
{
    public FunctionParameterMapper()
    {
        CreateMap<CreateFunctionParameterDto, FunctionParameterDto>()
            .ReverseMap();

        CreateMap<FunctionParameterNestedDto, FunctionParameter>()
            .ForMember(dest => dest.FunctionParameterId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Options)))
            .ReverseMap();

        CreateMap<CreateFunctionParameterDto, FunctionParameter>()
            .ForMember(dest => dest.FunctionParameterId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Options)))
            .ReverseMap();

        CreateMap<UpdateFunctionParameterDto, FunctionParameter>()
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Options)))
            .ReverseMap();

        CreateMap<IPaginate<FunctionParameterDto>, IPaginate<FunctionParameter>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<FunctionParameter, FunctionParameterDto>()
            .ForMember(dest => dest.Options,
                opt => opt.MapFrom(src => JsonConvert.DeserializeObject<List<ParameterOptionDto>>(src.Options)))
            .ReverseMap()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Options)));

        CreateMap<FunctionParameter, FunctionParameterInsideDto>()
            .ForMember(dest => dest.Options,
                opt => opt.MapFrom(src => JsonConvert.DeserializeObject<List<ParameterOptionDto>>(src.Options)))
            .ReverseMap()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Options)));
    }
}