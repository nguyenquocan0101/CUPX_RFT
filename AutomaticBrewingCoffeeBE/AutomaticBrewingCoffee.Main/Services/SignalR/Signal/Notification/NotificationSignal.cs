using AutomaticBrewingCoffee.Domain.Enums;

namespace Services.SignalR.Signal.Notification;

public class NotificationSignal
{
    public string NotificationId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public ESeverity Severity { get; set; }

    public string ReferenceId { get; set; } = null!;

    public string ReferenceType { get; set; } = null!;
}