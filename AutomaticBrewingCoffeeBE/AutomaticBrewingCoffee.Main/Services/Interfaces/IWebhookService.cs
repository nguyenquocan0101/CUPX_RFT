using AutomaticBrewingCoffee.Repository.Pagination;
using Services.Base;
using Services.Dtos.Webhook;

namespace Services.Interfaces;

public interface IWebhookService
{
    Task<BaseResult<RegisterWebhookDto, WebhookDto>> RegisterWebhook(RegisterWebhookDto registerWebhookDto);
    Task<BaseResult<string, WebhookDto>> UpdateWebhook(string webhookId, UpdateWebhookDto updateWebhookDto);

    Task<BaseResult<string, WebhookDto>> GetWebhook(string webhookId);
    Task<BaseResult<string, Paginate<WebhookDto>>> GetWebhooks(WebhookQueryDto webhookQueryDto);
}