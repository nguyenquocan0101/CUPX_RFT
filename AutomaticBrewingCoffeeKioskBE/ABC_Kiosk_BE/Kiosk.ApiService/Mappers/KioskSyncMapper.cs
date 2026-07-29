using AutoMapper;
using Domain.CouchDbModels;
using Domain.Enums;
using Domain.Models;
using Services.Dtos.Menu;
using Services.Dtos.Sync;

namespace Kiosk.ApiService.Mappers
{
    public class KioskSyncMapper : Profile
    {
        public KioskSyncMapper()
        {
            CreateMap<DeviceSyncDto, Device>()
               .ReverseMap();
            CreateMap<DeviceSyncDto, DeviceDocument>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DeviceId)) // CouchDB dùng "Id"
            .ForMember(dest => dest.Rev, opt => opt.Ignore()); // Bỏ qua Rev khi mapping từ DTO
            CreateMap<DeviceSyncDto, DeviceStatusDocument>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DeviceId)) // CouchDB dùng "Id"
            .ForMember(dest => dest.Rev, opt => opt.Ignore()) // Bỏ qua Rev 
                .ReverseMap();

            CreateMap<ProductSyncDto, Product>()
                //.ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))
                //.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            CreateMap<WorkflowSyncDto, Workflow>()
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                 .ReverseMap();
            CreateMap<WorkflowSyncDto, WorkflowData>()
                 .ReverseMap();

            CreateMap<StepSyncDto, Step>()
                .ForMember(dest => dest.Function, opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            CreateMap<MenuSyncDto, Menu>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

            CreateMap<MenuProductMappingSyncDto, MenuProductMapping>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();

            CreateMap<StepConditionRaw, StepConditionRawSyncDto>()
               .ReverseMap();
        }
    }
}
