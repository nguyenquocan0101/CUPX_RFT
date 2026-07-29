using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Dtos.Device;
using Services.Dtos.Kiosk;
using Services.Utils;

namespace AutomaticBrewingCoffee.API.Mappers
{
    public class KioskMapper : Profile
    {
        public KioskMapper()
        {
            CreateMap<KioskInOrderDto, Kiosk>()
                .ReverseMap();

            CreateMap<KioskInSyncTaskDto, Kiosk>()
                .ReverseMap();

            CreateMap<CreateKioskDto, KioskDto>()
                .ReverseMap();

            CreateMap<Kiosk, KioskDto>()
                .ForMember(dest => dest.ApiKey, opt => opt.MapFrom(src => ApiKeyUtil.Decrypt(src.ApiKey!)))
                .ReverseMap();

            CreateMap<Device, DeviceDto>();

            CreateMap<CreateKioskDto, Kiosk>()
                .ForMember(dest => dest.KioskId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .AfterMap((src, dest, context) =>
                {
                    if (
                        context.Items.TryGetValue("ApiKey", out var value)
                        && value is string apiKey
                        && !string.IsNullOrWhiteSpace(apiKey)
                    )
                    {
                        dest.ApiKey =
                            ApiKeyUtil.Encrypt(apiKey);
                    }

                    if (
                        context.Items.TryGetValue("TunnelId", out var tunnelIdValue)
                        && tunnelIdValue is string tunnelId
                        && !string.IsNullOrWhiteSpace(tunnelId)
                    )
                    {
                        dest.KioskId = tunnelId;
                    }

                    if (
                        context.Items.TryGetValue("Hostname", out var hostnameValue)
                        && hostnameValue is string hostname
                        && !string.IsNullOrWhiteSpace(hostname)
                    )
                    {
                        dest.Hostname = hostname;
                    }

                    if (
                        context.Items.TryGetValue("OriginServer", out var originServerValue)
                        && hostnameValue is string originServer
                        && !string.IsNullOrWhiteSpace(originServer)
                    )
                    {
                        dest.OriginServer = originServer;
                    }
                })
                .ReverseMap();

            CreateMap<UpdateKioskDto, Kiosk>()
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ReverseMap();

            CreateMap<IPaginate<KioskDto>, IPaginate<Kiosk>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
                .ReverseMap();

            CreateMap<Device, DeviceDto>().ReverseMap();
        }
    }
}