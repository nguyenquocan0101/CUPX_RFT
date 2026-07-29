using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Services.Local;

public sealed class SqlLocalWebhookPersistence(AutoBrewingBeContext dbContext) : ILocalWebhookPersistence
{
    public async Task<LocalWebhookPersistenceRecord?> FindAsync(
        string source,
        string eventType,
        string eventId,
        CancellationToken cancellationToken)
    {
        var inbox = await dbContext.LocalWebhookInboxes
            .Include(x => x.Outbox)
            .SingleOrDefaultAsync(
                x => x.Source == source && x.EventType == eventType && x.EventId == eventId,
                cancellationToken);

        return inbox?.Outbox is null ? null : new LocalWebhookPersistenceRecord(inbox, inbox.Outbox);
    }

    public async Task<LocalWebhookPersistenceRecord> CreateAsync(
        LocalWebhookTriggerRequest request,
        string payloadHash,
        string idempotencyKey,
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var inbox = new LocalWebhookInbox
        {
            InboxId = idempotencyKey,
            Source = request.Source,
            EventType = request.EventType,
            EventId = request.EventId,
            PayloadHash = payloadHash,
            IdempotencyKey = idempotencyKey,
            Status = LocalWebhookStatus.Pending
        };
        var outbox = new LocalWebhookOutbox
        {
            OutboxId = idempotencyKey,
            InboxId = inbox.InboxId,
            Inbox = inbox,
            TargetPath = request.Path,
            HttpMethod = request.HttpMethod,
            PayloadJson = payloadJson,
            Status = LocalWebhookStatus.Pending
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.LocalWebhookInboxes.Add(inbox);
        dbContext.LocalWebhookOutboxes.Add(outbox);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new LocalWebhookPersistenceRecord(inbox, outbox);
    }

    public async Task<bool> TryClaimAsync(
        string inboxId,
        DateTime now,
        DateTime leaseUntil,
        CancellationToken cancellationToken)
    {
        var updated = await dbContext.LocalWebhookOutboxes
            .Where(x => x.InboxId == inboxId
                && x.Status != LocalWebhookStatus.Succeeded
                && (x.Status != LocalWebhookStatus.Processing || x.LeaseUntil == null || x.LeaseUntil < now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, LocalWebhookStatus.Processing)
                .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1)
                .SetProperty(x => x.LeaseUntil, leaseUntil)
                .SetProperty(x => x.LastError, (string?)null), cancellationToken);

        if (updated == 0)
            return false;

        await dbContext.LocalWebhookInboxes
            .Where(x => x.InboxId == inboxId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, LocalWebhookStatus.Processing)
                .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1)
                .SetProperty(x => x.LastAttemptAt, now)
                .SetProperty(x => x.LeaseUntil, leaseUntil)
                .SetProperty(x => x.LastError, (string?)null), cancellationToken);
        return true;
    }

    public async Task MarkSucceededAsync(
        string inboxId,
        int statusCode,
        DateTime completedAt,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.LocalWebhookInboxes
            .Where(x => x.InboxId == inboxId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, LocalWebhookStatus.Succeeded)
                .SetProperty(x => x.StatusCode, statusCode)
                .SetProperty(x => x.CompletedAt, completedAt)
                .SetProperty(x => x.LeaseUntil, (DateTime?)null)
                .SetProperty(x => x.LastError, (string?)null), cancellationToken);
        await dbContext.LocalWebhookOutboxes
            .Where(x => x.InboxId == inboxId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, LocalWebhookStatus.Succeeded)
                .SetProperty(x => x.LastStatusCode, statusCode)
                .SetProperty(x => x.SentAt, completedAt)
                .SetProperty(x => x.LeaseUntil, (DateTime?)null)
                .SetProperty(x => x.LastError, (string?)null), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        string inboxId,
        int statusCode,
        string error,
        CancellationToken cancellationToken)
    {
        var truncatedError = error.Length > 2000 ? error[..2000] : error;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.LocalWebhookInboxes
            .Where(x => x.InboxId == inboxId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, LocalWebhookStatus.Failed)
                .SetProperty(x => x.StatusCode, statusCode)
                .SetProperty(x => x.LeaseUntil, (DateTime?)null)
                .SetProperty(x => x.LastError, truncatedError), cancellationToken);
        await dbContext.LocalWebhookOutboxes
            .Where(x => x.InboxId == inboxId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, LocalWebhookStatus.Failed)
                .SetProperty(x => x.LastStatusCode, statusCode)
                .SetProperty(x => x.LeaseUntil, (DateTime?)null)
                .SetProperty(x => x.LastError, truncatedError), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
