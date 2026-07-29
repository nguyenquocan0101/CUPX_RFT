using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Payment;

namespace AutomaticBrewingCoffee.API.Mappers;

public class PaymentMapper : Profile
{
    public PaymentMapper()
    {
        CreateMap<Payment, PaymentDto>()
            .ReverseMap();
    }
}