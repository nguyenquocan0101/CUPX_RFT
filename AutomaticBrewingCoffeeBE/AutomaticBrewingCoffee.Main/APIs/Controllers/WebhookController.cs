using AutomaticBrewingCoffee.API.Attribute;
using AutomaticBrewingCoffee.API.Constants;
using AutomaticBrewingCoffee.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Services.Base;
using Services.Dtos.Webhook;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace AutomaticBrewingCoffee.API.Controllers
{
    [Route($"{ApiEndpointsConstant.API_ENDPOINT}/webhooks")]
    [ApiController]
    [TrimStrings]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _webhookService;

        public WebhookController(IWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        [HttpPost]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Register a webhook for kiosk",
            Description = "Define: referenceId => kiosk id, WebhookType => SynchronizedData | ExecuteProduct"
        )]
        public async Task<ActionResult<BaseResult<RegisterWebhookDto, WebhookDto>>> Post(
            RegisterWebhookDto registerWebhookDto)
        {
            var response = await _webhookService.RegisterWebhook(registerWebhookDto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{webhookId}")]
        [Authorizes(nameof(ERoleName.Admin))]
        [SwaggerOperation(
            Summary = "Update a webhook for kiosk",
            Description = "Define: referenceId => kiosk id, WebhookType => SynchronizedData | ExecuteProduct"
        )]
        public async Task<ActionResult<BaseResult<string, WebhookDto>>> Put(
            [FromRoute] string webhookId,
            [FromBody] UpdateWebhookDto updateWebhookDto)
        {
            var response = await _webhookService.UpdateWebhook(webhookId, updateWebhookDto);
            return StatusCode(response.StatusCode, response);
        }
    }
}