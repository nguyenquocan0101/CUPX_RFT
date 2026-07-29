using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Services.Local;

namespace Services.Tests.Local.EmailWebhook;

public sealed class LocalWebhookTriggerTests
{
    [Fact]
    public async Task TriggerAsync_PostsToLocalhostWithApiKeyAndDeterministicIdempotencyKey()
    {
        var handler = new RecordingHttpHandler();
        using var httpClient = new HttpClient(handler);
        var trigger = new LocalWebhookTrigger(
            httpClient,
            new LocalWebhookOptions { BaseUrl = "http://localhost:5160", ApiKey = "local-key" });
        var request = CreateRequest();

        var first = await trigger.TriggerAsync(request);
        var replay = await trigger.TriggerAsync(request);

        Assert.True(first.IsSuccess);
        Assert.False(first.IsReplay);
        Assert.True(replay.IsReplay);
        Assert.Equal(first.IdempotencyKey, replay.IdempotencyKey);
        Assert.Single(handler.Requests);
        Assert.Equal("http://localhost:5160/api/v1/execute", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal("local-key", handler.Requests[0].Headers.GetValues("X-API-Key").Single());
        Assert.Equal(first.IdempotencyKey, handler.Requests[0].Headers.GetValues("Idempotency-Key").Single());
    }

    [Fact]
    public async Task TriggerAsync_RejectsNonLoopbackTargetBeforeSending()
    {
        var handler = new RecordingHttpHandler();
        using var httpClient = new HttpClient(handler);
        var trigger = new LocalWebhookTrigger(
            httpClient,
            new LocalWebhookOptions { BaseUrl = "https://example.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => trigger.TriggerAsync(CreateRequest()));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task TriggerAsync_ReturnsConflictForSameEventWithDifferentPayload()
    {
        var handler = new RecordingHttpHandler();
        using var httpClient = new HttpClient(handler);
        var trigger = new LocalWebhookTrigger(
            httpClient,
            new LocalWebhookOptions { BaseUrl = "http://127.0.0.1:5160" });

        var first = await trigger.TriggerAsync(CreateRequest("first"));
        var conflict = await trigger.TriggerAsync(CreateRequest("second"));

        Assert.True(first.IsSuccess);
        Assert.False(conflict.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, (HttpStatusCode)conflict.StatusCode);
        Assert.False(conflict.IsReplay);
        Assert.Single(handler.Requests);
    }

    private static LocalWebhookTriggerRequest CreateRequest(string value = "payload") =>
        new()
        {
            Source = "main",
            EventType = "ExecuteProduct",
            EventId = "event-001",
            Path = "/api/v1/execute",
            Payload = JsonSerializer.SerializeToElement(new { value })
        };

    private sealed class RecordingHttpHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { ok = true })
            });
        }
    }
}
