namespace Services.Dtos.Webhook;

public class WebhookDto
{
    public string WebhookId { get; set; } = null!;

    public string? ReferenceId { get; set; }

    public string? WebhookUrl { get; set; }

    public string? WebhookType { get; set; }
}