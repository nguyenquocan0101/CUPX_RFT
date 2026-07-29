using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using Services.Dtos.Webhook;

namespace AutomaticBrewingCoffee.API.Mappers;

public class WebhookMapper : Profile
{
    public WebhookMapper()
    {
        CreateMap<WebhookDto, Webhook>()
            .ReverseMap();

        CreateMap<RegisterWebhookDto, Webhook>()
            .ForMember(dest => dest.WebhookId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
            .ReverseMap();

        CreateMap<UpdateWebhookDto, Webhook>()
            .ReverseMap();
    }
}