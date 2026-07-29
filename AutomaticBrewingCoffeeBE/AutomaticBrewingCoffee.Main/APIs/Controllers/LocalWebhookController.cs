using System.Security.Cryptography;
using System.Text;
using AutomaticBrewingCoffee.API.Constants;
using Microsoft.AspNetCore.Mvc;
using Services.Local;

namespace AutomaticBrewingCoffee.API.Controllers;

[ApiController]
[Route($"{ApiEndpointsConstant.API_ENDPOINT}/local-webhooks")]
public sealed class LocalWebhookController : ControllerBase
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly LocalWebhookTrigger _trigger;

    public LocalWebhookController(
        IHostEnvironment environment,
        IConfiguration configuration,
        LocalWebhookTrigger trigger)
    {
        _environment = environment;
        _configuration = configuration;
        _trigger = trigger;
    }

    [HttpPost("trigger")]
    public async Task<ActionResult<LocalWebhookTriggerResult>> Trigger(
        [FromBody] LocalWebhookTriggerRequest request,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsEnvironment("Local"))
            return NotFound();
        if (!HasLocalApiKey())
            return Unauthorized();

        var result = await _trigger.TriggerAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private bool HasLocalApiKey()
    {
        var expected = _configuration["LocalSeed:KioskApiKey"];
        var received = Request.Headers["X-API-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(received))
            return false;
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(received));
    }
}
