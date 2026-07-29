using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.SignalR;
using Services.SignalR.Base;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route($"{ApiEndpointsConstant.API_ENDPOINT}/test")]
[TrimStrings]
public class TestController : ControllerBase
{
    private readonly IHubContext<OrderHub> _hubContext;
    private ILogger<TestController> _logger;

    public TestController(IHubContext<OrderHub> hubContext, ILogger<TestController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }


    [HttpPost("push-trans-status-to-tablet")]
    [SwaggerOperation(
        Summary = "Status = Success | Fail; ClientId = {DeviceId}"
    )]
    public async Task<IActionResult> PushTransStatus(string clientId, string status)
    {
        if (OrderHub.ClientIdToConnectionId.TryGetValue(clientId, out var connectionId))
        {
            await _hubContext.Clients.Client(connectionId).SendAsync(SignalREvents.ReceiveTrans, status);
            _logger.LogInformation("PaymentHub.PushTransactionStatus: {ClientId} | {Status}", clientId, status);
        }
        else
        {
            _logger.LogWarning("PaymentHub.PushTransactionStatus: ClientId not found");
        }

        return Ok();
    }
}