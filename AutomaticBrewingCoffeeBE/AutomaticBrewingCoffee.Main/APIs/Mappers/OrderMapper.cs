using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Newtonsoft.Json;
using Services.Dtos.Order;

namespace AutomaticBrewingCoffee.API.Mappers;

public class OrderMapper : Profile
{
    public OrderMapper()
    {
        CreateMap<ExportOrderDto, Order>().ReverseMap();

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
            .AfterMap((src, dest, context) =>
            {
                foreach (var detail in dest.OrderDetails)
                {
                    detail.OrderId = dest.OrderId;
                }

                if (context.Items.TryGetValue("StoreId", out var storeId))
                {
                    dest.StoreId = storeId.ToString();
                }

                if (context.Items.TryGetValue("OrganizationId", out var organizationId))
                {
                    dest.OrganizationId = organizationId.ToString();
                }

                if (context.Items.TryGetValue("KioskId", out var kioskId))
                {
                    dest.KioskId = kioskId.ToString()!;
                }

                if (context.Items.TryGetValue("OrderType", out var orderType))
                {
                    dest.OrderType = orderType.ToString()!;
                }

                if (context.Items.TryGetValue("OrderCode", out var orderCode))
                {
                    dest.OrderCode = orderCode.ToString()!;
                }
                else
                {
                    dest.OrderCode = $"ORD{dest.CreatedDate:yyyyMMddHHmm}";
                }
            })
            .ReverseMap();

        CreateMap<IPaginate<OrderDto>, IPaginate<Order>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ReverseMap();

        CreateMap<Order, OrderInsideDto>()
            .ReverseMap();

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.PreparingProducts,
                opt => opt.MapFrom(src =>
                    src.PreparingProductIds == null
                        ? null
                        : JsonConvert.DeserializeObject(src.PreparingProductIds)))
            .ForMember(dest => dest.CompletedProducts,
                opt => opt.MapFrom(src =>
                    src.CompletedProductIds == null
                        ? null
                        : JsonConvert.DeserializeObject(src.CompletedProductIds)))
            .ForMember(dest => dest.FailedProducts,
                opt => opt.MapFrom(src =>
                    src.FailedProductIds == null
                        ? null
                        : JsonConvert.DeserializeObject(src.FailedProductIds)))
            .AfterMap((src, dest, context) =>
            {
                if (context.Items.TryGetValue("PaymentId", out var paymentId))
                {
                    dest.PaymentId = paymentId.ToString();
                }

                if (context.Items.TryGetValue("PaymentUrl", out var paymentUrl))
                {
                    dest.PaymentUrl = paymentUrl.ToString();
                }

                if (context.Items.TryGetValue("PaymentQr", out var paymentQr))
                {
                    dest.PaymentQr = paymentQr.ToString();
                }

                if (context.Items.TryGetValue("ExpiredDate", out var expiredDate))
                {
                    dest.ExpiredDate = (DateTime)expiredDate;
                }
            })
            .ReverseMap();
    }
}