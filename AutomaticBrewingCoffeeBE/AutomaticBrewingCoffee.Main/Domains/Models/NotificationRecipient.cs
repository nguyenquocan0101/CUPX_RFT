using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class NotificationRecipient : BaseModel
{
    [Key] [StringLength(50)] [Required] public string NotificationRecipientId { get; set; } = null!;

    [StringLength(50)] [Required] public string NotificationId { get; set; } = null!;

    [ForeignKey(nameof(NotificationId))] public Notification Notification { get; set; } = null!;

    [StringLength(50)] public string AccountRole { get; set; } = null!;

    [StringLength(50)] public string AccountId { get; set; } = null!;

    [ForeignKey(nameof(AccountId))] public Account Account { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadDate { get; set; }

    public void Read()
    {
        IsRead = true;
        ReadDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }
}