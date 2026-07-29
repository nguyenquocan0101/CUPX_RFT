using Services.Dtos.Account;
using Services.Dtos.Notification;

namespace Services.Dtos.NotificationRecipient;

public class NotificationRecipientDto
{
    public string NotificationRecipientId { get; set; } = null!;

    public string NotificationId { get; set; } = null!;

    public string AccountRole { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public AccountDto Account { get; set; } = null!;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadDate { get; set; }
    
    public DateTime CreatedDate { get; set; } 
    
    public DateTime? UpdatedDate { get; set; } = null!;
}