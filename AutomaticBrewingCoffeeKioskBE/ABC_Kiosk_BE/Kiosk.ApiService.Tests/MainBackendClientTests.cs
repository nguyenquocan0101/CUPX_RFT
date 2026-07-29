using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services.ExternalClients;

namespace Kiosk.ApiService.Tests;

public class MainBackendClientTests
{
    [Fact]
    public async Task Complete_order_uses_local_main_backend_and_outbound_api_key()
    {
        var handler = new RecordingHttpMessageHandler
        {
            Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"isSuccess\":true}", Encoding.UTF8, "application/json")
            }
        };
        var httpClient = new HttpClient(handler);
        var client = new MainBackendClient(
            httpClient,
            Options.Create(new MainBackendOptions
            {
                BaseUrl = "http://localhost:5100",
                OrdersEndpoint = "/api/v1/orders",
                OutboundApiKey = "outbound-local-key"
            }),
            NullLogger<MainBackendClient>.Instance);

        var result = await client.CompleteOrderAsync("order-1", ["product-1"]);

        Assert.True(result);
        Assert.Equal(HttpMethod.Put, handler.Request!.Method);
        Assert.Equal("http://localhost:5100/api/v1/orders/complete", handler.Request.RequestUri!.ToString());
        Assert.Equal("outbound-local-key", handler.Request.Headers.GetValues("X-API-Key").Single());
        Assert.Contains("order-1", handler.RequestBody);
        Assert.Contains("Completed", handler.RequestBody);
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string RequestBody { get; private set; } = string.Empty;
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return Response;
        }
    }
}
