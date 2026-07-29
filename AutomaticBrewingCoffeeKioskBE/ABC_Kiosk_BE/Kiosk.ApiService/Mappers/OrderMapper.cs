using System.Text.Json;
using AutoMapper;
using Domain.Models;
using Domain.Pagination;
using Services.Dtos.Order;
using Services.ExternalClients;

namespace AutomaticBrewingCoffee.API.Mappers;

public class OrderMapper : Profile
{
    public OrderMapper()
    {
        CreateMap<CreateLocalOrderDto, LocalOrder>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            //.ForMember(dest => dest.LocalOrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
            //.AfterMap((src, dest) =>
            //{
            //    foreach (var detail in dest.LocalOrderDetails)
            //    {
            //        detail.OrderId = dest.OrderId;
            //        detail.OrderDetailId = Guid.NewGuid().ToString();
            //        detail.DetailData = detail.ToString();
            //    }
            //})
            .ReverseMap();

        CreateMap<IPaginate<PrepareOrderDto>, IPaginate<LocalOrder>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<LocalOrder, PrepareOrderDto>()
            .ReverseMap();

        //CreateMap<CloudOrderPaymentResponse, PrepareOrderDto>();

        //CreateMap<LocalOrder, LocalOrderDto>()
        //    .ForMember(des => des.OrderData, opt => opt.Ignore())
        //    .AfterMap((src, des) =>
        //    {
        //        des.OrderData = JsonSerializer.Deserialize<CloudOrderPaymentResponse>(src.OrderData ?? "{}");
        //    });


    }
}