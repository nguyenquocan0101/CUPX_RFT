using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public sealed class LocalWebhookOutbox
{
    [Key]
    [StringLength(64)]
    public string OutboxId { get; set; } = null!;

    [Required]
    [StringLength(64)]
    public string InboxId { get; set; } = null!;

    [ForeignKey(nameof(InboxId))]
    public LocalWebhookInbox Inbox { get; set; } = null!;

    [Required]
    [StringLength(450)]
    public string TargetPath { get; set; } = null!;

    [Required]
    [StringLength(16)]
    public string HttpMethod { get; set; } = "POST";

    [Required]
    public string PayloadJson { get; set; } = null!;

    [Required]
    [StringLength(32)]
    public string Status { get; set; } = LocalWebhookStatus.Pending;

    public int? LastStatusCode { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public DateTime? LeaseUntil { get; set; }

    [StringLength(2000)]
    public string? LastError { get; set; }
}
