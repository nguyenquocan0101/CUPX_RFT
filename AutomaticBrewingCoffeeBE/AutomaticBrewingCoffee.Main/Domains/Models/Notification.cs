using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Notification : BaseModel
{
    [Key] [StringLength(50)] [Required] public string NotificationId { get; set; } = null!;

    [StringLength(100)] public string Title { get; set; } = null!;

    [StringLength(2048)] public string Message { get; set; } = null!;

    [StringLength(50)] public string NotificationType { get; set; } = null!;

    [StringLength(50)] public string? ReferenceId { get; set; }

    [StringLength(50)] public string? ReferenceType { get; set; }

    [StringLength(50)] public string? Severity { get; set; }

    [StringLength(50)] public string CreatedBy { get; set; } = null!;

    public ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
}