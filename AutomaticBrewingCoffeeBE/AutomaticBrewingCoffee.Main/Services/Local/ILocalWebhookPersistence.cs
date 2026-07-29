using AutomaticBrewingCoffee.Domain.Models;

namespace Services.Local;

public sealed record LocalWebhookPersistenceRecord(
    LocalWebhookInbox Inbox,
    LocalWebhookOutbox Outbox);

public interface ILocalWebhookPersistence
{
    Task<LocalWebhookPersistenceRecord?> FindAsync(
        string source,
        string eventType,
        string eventId,
        CancellationToken cancellationToken);

    Task<LocalWebhookPersistenceRecord> CreateAsync(
        LocalWebhookTriggerRequest request,
        string payloadHash,
        string idempotencyKey,
        string payloadJson,
        CancellationToken cancellationToken);

    Task<bool> TryClaimAsync(
        string inboxId,
        DateTime now,
        DateTime leaseUntil,
        CancellationToken cancellationToken);

    Task MarkSucceededAsync(
        string inboxId,
        int statusCode,
        DateTime completedAt,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        string inboxId,
        int statusCode,
        string error,
        CancellationToken cancellationToken);
}
