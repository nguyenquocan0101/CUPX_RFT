using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.NotificationRecipient;

namespace AutomaticBrewingCoffee.API.Mappers;

public class NotificationRecipientMapper : Profile
{
    public NotificationRecipientMapper()
    {
        CreateMap<NotificationRecipient, NotificationRecipientDto>().ReverseMap();
    }
}