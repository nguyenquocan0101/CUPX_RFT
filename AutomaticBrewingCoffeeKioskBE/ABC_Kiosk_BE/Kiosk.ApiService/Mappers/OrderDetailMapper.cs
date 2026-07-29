using AutoMapper;
using Domain.Models;
using Services.Dtos.Order;
using Services.ExternalClients;
namespace AutomaticBrewingCoffee.API.Mappers;

public class OrderDetailMapper : Profile
{
    public OrderDetailMapper()
    {
        CreateMap<CreateLocalOrderDetailDto, LocalOrderDetail>()
            .ForMember(dest => dest.OrderDetailId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap();

        CreateMap<LocalOrderDetailDto, LocalOrderDetail>()
            .ReverseMap();

       // CreateMap<OrderDetailDto, LocalOrderDetailDto>();
    }
}