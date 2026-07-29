namespace Services.Dtos.Notification;

public class NotificationInsideDto
{
    public string NotificationId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public string? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public string? Severity { get; set; }

    public string CreatedBy { get; set; } = null!;
}