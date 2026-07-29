using Services.Dtos.NotificationRecipient;

namespace Services.Dtos.Notification;

public class NotificationDto
{
    public string NotificationId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public string? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public string? Severity { get; set; }

    public string CreatedBy { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; } = null!;

    public ICollection<NotificationRecipientDto> NotificationRecipients { get; set; } =
        new List<NotificationRecipientDto>();
}