using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Newtonsoft.Json;
using Services.Dtos.Step;
using Services.Dtos.Sync;

namespace AutomaticBrewingCoffee.API.Mappers;

public class SyncMapper : Profile
{
    public SyncMapper()
    {
        CreateMap<ProductSyncDto, Product>()
            .ReverseMap()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

        CreateMap<MenuSyncDto, Menu>()
            .ReverseMap()
            .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.MenuId));

        CreateMap<WorkflowSyncDto, Workflow>()
            .ReverseMap()
            .ForMember(dest => dest.WorkflowId, opt => opt.MapFrom(src => src.WorkflowId));

        CreateMap<StepSyncDto, Step>()
            .ReverseMap()
            .ForMember(dest => dest.StepId, opt => opt.MapFrom(src => src.StepId))
            .ForMember(dest => dest.Conditions,
                opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.Conditions)
                        ? null
                        : JsonConvert.DeserializeObject<List<StepConditionRawDto>>(src.Conditions)));

        CreateMap<DeviceSyncDto, Device>()
            .ReverseMap()
            .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.DeviceId));


        CreateMap<MenuProductMappingSyncDto, MenuProductMapping>().ReverseMap();
    }
}