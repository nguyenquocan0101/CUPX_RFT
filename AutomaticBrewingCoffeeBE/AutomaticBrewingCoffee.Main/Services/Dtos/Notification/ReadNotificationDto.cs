using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Notification;

public class ReadNotificationDto
{
    [StringLength(50)] [Required] public string NotificationId { get; set; } = null!;
}