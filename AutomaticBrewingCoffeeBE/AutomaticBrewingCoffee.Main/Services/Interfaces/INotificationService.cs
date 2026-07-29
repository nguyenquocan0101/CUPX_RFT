using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Notification;

namespace Services.Interfaces;

public interface INotificationService
{
    Task<BaseResult<NotificationQueryDto, Paginate<NotificationDto>>> GetNotifications(
        NotificationQueryDto notificationQueryDto);

    Task<BaseResult<string, NotificationDto>> GetNotification(string notificationId);

    Task<BaseResult<ReadNotificationDto, NotificationDto>> ReadNotification(ReadNotificationDto readNotificationDto);

    Task<BaseResult<ReadNotificationsDto, List<NotificationDto>>> ReadNotifications(
        ReadNotificationsDto readNotificationsDto);
}