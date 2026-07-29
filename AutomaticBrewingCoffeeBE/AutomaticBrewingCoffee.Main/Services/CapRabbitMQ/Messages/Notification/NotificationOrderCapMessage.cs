using AutomaticBrewingCoffee.Domain.Enums;

namespace Services.CapRabbitMQ.Messages.Notification;

public class NotificationOrderCapMessage
{
    public ENotificationType NotificationType { get; set; }

    public string? OrderId { get; set; }

    public string CreatedBy { get; set; } = null!;
}