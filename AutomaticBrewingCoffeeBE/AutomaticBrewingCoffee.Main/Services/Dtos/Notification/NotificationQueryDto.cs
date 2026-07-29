using Services.Base;

namespace Services.Dtos.Notification;

public class NotificationQueryDto : BaseQuery
{
    public bool? IsRead { get; set; }

    public string? AccountRole { get; set; }

    public string? AccountId { get; set; }

    public string? NotificationType { get; set; }

    public string? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public string? Severity { get; set; }
    
    public string? CreatedBy { get; set; } = null!;
}