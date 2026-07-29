using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Services.Local;

public sealed class LocalWebhookOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5160";
    public string ApiKey { get; set; } = string.Empty;
}

public sealed class LocalWebhookTriggerRequest
{
    public string Source { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST";
    public JsonElement Payload { get; set; }
}

public sealed record LocalWebhookTriggerResult(bool IsSuccess, bool IsReplay, int StatusCode, string IdempotencyKey);

public sealed class LocalWebhookTrigger
{
    private readonly HttpClient _httpClient;
    private readonly LocalWebhookOptions _options;
    private readonly ILocalWebhookPersistence _persistence;

    public LocalWebhookTrigger(HttpClient httpClient, LocalWebhookOptions options)
        : this(httpClient, options, new InMemoryLocalWebhookPersistence())
    {
    }

    public LocalWebhookTrigger(
        HttpClient httpClient,
        LocalWebhookOptions options,
        ILocalWebhookPersistence persistence)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    public async Task<LocalWebhookTriggerResult> TriggerAsync(LocalWebhookTriggerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var baseUri = new Uri(_options.BaseUrl, UriKind.Absolute);
        if (!baseUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || !IsLoopback(baseUri.Host))
            throw new InvalidOperationException("Local webhooks may target HTTP loopback addresses only.");

        var payload = request.Payload.GetRawText();
        var eventKey = $"{request.Source}:{request.EventType}:{request.EventId}";
        var fingerprint = ComputeHash(payload);
        var idempotencyKey = ComputeHash(eventKey);

        var persisted = await _persistence.FindAsync(
            request.Source,
            request.EventType,
            request.EventId,
            cancellationToken);
        if (persisted is not null)
        {
            if (!string.Equals(persisted.Inbox.PayloadHash, fingerprint, StringComparison.Ordinal))
                return new LocalWebhookTriggerResult(false, false, 409, idempotencyKey);

            if (persisted.Inbox.Status == LocalWebhookStatus.Succeeded)
                return new LocalWebhookTriggerResult(
                    true,
                    true,
                    persisted.Inbox.StatusCode ?? 200,
                    persisted.Inbox.IdempotencyKey);
        }
        else
        {
            try
            {
                persisted = await _persistence.CreateAsync(
                    request,
                    fingerprint,
                    idempotencyKey,
                    payload,
                    cancellationToken);
            }
            catch (DbUpdateException)
            {
                persisted = await _persistence.FindAsync(
                    request.Source,
                    request.EventType,
                    request.EventId,
                    cancellationToken);
                if (persisted is null)
                    throw;
                if (!string.Equals(persisted.Inbox.PayloadHash, fingerprint, StringComparison.Ordinal))
                    return new LocalWebhookTriggerResult(false, false, 409, idempotencyKey);
                if (persisted.Inbox.Status == LocalWebhookStatus.Succeeded)
                    return new LocalWebhookTriggerResult(
                        true,
                        true,
                        persisted.Inbox.StatusCode ?? 200,
                        persisted.Inbox.IdempotencyKey);
            }
        }

        if (persisted is null)
            throw new InvalidOperationException("Local webhook persistence did not return an event record.");

        var now = DateTime.UtcNow;
        if (!await _persistence.TryClaimAsync(
                persisted.Inbox.InboxId,
                now,
                now.AddMinutes(1),
                cancellationToken))
        {
            return new LocalWebhookTriggerResult(false, false, 409, idempotencyKey);
        }

        var method = new HttpMethod(persisted.Outbox.HttpMethod);
        using var message = new HttpRequestMessage(
            method,
            new Uri(baseUri, persisted.Outbox.TargetPath.TrimStart('/')));
        if (method != HttpMethod.Get && method != HttpMethod.Head)
            message.Content = JsonContent.Create(request.Payload);
        message.Headers.Add("Idempotency-Key", idempotencyKey);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            message.Headers.Add("X-API-Key", _options.ApiKey);

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await _persistence.MarkFailedAsync(
                    persisted.Inbox.InboxId,
                    (int)response.StatusCode,
                    $"Target returned HTTP {(int)response.StatusCode}.",
                    cancellationToken);
                return new LocalWebhookTriggerResult(false, false, (int)response.StatusCode, idempotencyKey);
            }
            await _persistence.MarkSucceededAsync(
                persisted.Inbox.InboxId,
                (int)response.StatusCode,
                DateTime.UtcNow,
                cancellationToken);
            return new LocalWebhookTriggerResult(true, false, (int)response.StatusCode, idempotencyKey);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await _persistence.MarkFailedAsync(
                persisted.Inbox.InboxId,
                0,
                exception.Message,
                cancellationToken);
            throw;
        }
    }

    private static bool IsLoopback(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address);

    private static string ComputeHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}

internal sealed class InMemoryLocalWebhookPersistence : ILocalWebhookPersistence
{
    private readonly Dictionary<string, LocalWebhookPersistenceRecord> _records = new(StringComparer.Ordinal);
    private readonly object _sync = new();

    public Task<LocalWebhookPersistenceRecord?> FindAsync(
        string source,
        string eventType,
        string eventId,
        CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            _records.TryGetValue($"{source}:{eventType}:{eventId}", out var record);
            return Task.FromResult(record);
        }
    }

    public Task<LocalWebhookPersistenceRecord> CreateAsync(
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
            IdempotencyKey = idempotencyKey
        };
        var outbox = new LocalWebhookOutbox
        {
            OutboxId = idempotencyKey,
            InboxId = idempotencyKey,
            Inbox = inbox,
            TargetPath = request.Path,
            HttpMethod = request.HttpMethod,
            PayloadJson = payloadJson
        };
        var record = new LocalWebhookPersistenceRecord(inbox, outbox);
        lock (_sync)
            _records[$"{request.Source}:{request.EventType}:{request.EventId}"] = record;
        return Task.FromResult(record);
    }

    public Task<bool> TryClaimAsync(
        string inboxId,
        DateTime now,
        DateTime leaseUntil,
        CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            var record = _records.Values.SingleOrDefault(x => x.Inbox.InboxId == inboxId);
            if (record is null || record.Outbox.Status == LocalWebhookStatus.Succeeded)
                return Task.FromResult(false);
            if (record.Outbox.Status == LocalWebhookStatus.Processing && record.Outbox.LeaseUntil > now)
                return Task.FromResult(false);
            record.Inbox.Status = LocalWebhookStatus.Processing;
            record.Inbox.AttemptCount++;
            record.Inbox.LastAttemptAt = now;
            record.Inbox.LeaseUntil = leaseUntil;
            record.Outbox.Status = LocalWebhookStatus.Processing;
            record.Outbox.AttemptCount++;
            record.Outbox.LeaseUntil = leaseUntil;
            return Task.FromResult(true);
        }
    }

    public Task MarkSucceededAsync(string inboxId, int statusCode, DateTime completedAt, CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            var record = _records.Values.Single(x => x.Inbox.InboxId == inboxId);
            record.Inbox.Status = LocalWebhookStatus.Succeeded;
            record.Inbox.StatusCode = statusCode;
            record.Inbox.CompletedAt = completedAt;
            record.Inbox.LeaseUntil = null;
            record.Outbox.Status = LocalWebhookStatus.Succeeded;
            record.Outbox.LastStatusCode = statusCode;
            record.Outbox.SentAt = completedAt;
            record.Outbox.LeaseUntil = null;
        }
        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(string inboxId, int statusCode, string error, CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            var record = _records.Values.Single(x => x.Inbox.InboxId == inboxId);
            record.Inbox.Status = LocalWebhookStatus.Failed;
            record.Inbox.StatusCode = statusCode;
            record.Inbox.LastError = error;
            record.Inbox.LeaseUntil = null;
            record.Outbox.Status = LocalWebhookStatus.Failed;
            record.Outbox.LastStatusCode = statusCode;
            record.Outbox.LastError = error;
            record.Outbox.LeaseUntil = null;
        }
        return Task.CompletedTask;
    }
}
