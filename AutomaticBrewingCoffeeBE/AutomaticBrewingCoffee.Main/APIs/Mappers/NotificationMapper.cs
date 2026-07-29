using AutoMapper;
using Services.Dtos.Notification;
using Notification = AutomaticBrewingCoffee.Domain.Models.Notification;

namespace AutomaticBrewingCoffee.API.Mappers;

public class NotificationMapper : Profile
{
    public NotificationMapper()
    {
        CreateMap<Notification, NotificationDto>()
            .AfterMap((src, dest, context) =>
            {
                if (!context.Items.TryGetValue("CurrentAccountId", out var currentAccountId)) return;

                var notificationRecipient =
                    src.NotificationRecipients.FirstOrDefault(x => x.AccountId == (string)currentAccountId);
                if (notificationRecipient == null) return;

                dest.IsRead = notificationRecipient.IsRead;
                dest.ReadDate = notificationRecipient.ReadDate;
            })
            .ReverseMap();

        CreateMap<Notification, NotificationInsideDto>().ReverseMap();
    }
}