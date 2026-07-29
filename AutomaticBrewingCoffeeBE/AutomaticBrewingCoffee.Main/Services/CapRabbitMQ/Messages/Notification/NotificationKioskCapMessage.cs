using AutomaticBrewingCoffee.Domain.Enums;

namespace Services.CapRabbitMQ.Messages.Notification;

public class NotificationKioskCapMessage
{
    public ENotificationType NotificationType { get; set; }

    public string? KioskId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string? Delivery { get; set; }
}