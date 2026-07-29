using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public sealed class LocalWebhookInbox
{
    [Key]
    [StringLength(64)]
    public string InboxId { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Source { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string EventId { get; set; } = null!;

    [Required]
    [StringLength(64)]
    public string PayloadHash { get; set; } = null!;

    [Required]
    [StringLength(64)]
    public string IdempotencyKey { get; set; } = null!;

    [Required]
    [StringLength(32)]
    public string Status { get; set; } = LocalWebhookStatus.Pending;

    public int? StatusCode { get; set; }

    public int AttemptCount { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime? LastAttemptAt { get; set; }

    public DateTime? LeaseUntil { get; set; }

    [StringLength(2000)]
    public string? LastError { get; set; }

    public LocalWebhookOutbox? Outbox { get; set; }
}

public static class LocalWebhookStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
}
