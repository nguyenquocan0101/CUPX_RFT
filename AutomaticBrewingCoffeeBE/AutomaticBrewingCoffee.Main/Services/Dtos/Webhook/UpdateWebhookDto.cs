using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Webhook;

public class UpdateWebhookDto
{
    [StringLength(450)] public string? WebhookUrl { get; set; }
}