using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Webhook;
using Services.Interfaces;
using Services.Utils;

namespace Services.Implements;

public class WebhookService : BaseService<WebhookService>, IWebhookService
{
    public WebhookService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
    {
    }

    public async Task<BaseResult<RegisterWebhookDto, WebhookDto>> RegisterWebhook(RegisterWebhookDto registerWebhookDto)
    {
        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == registerWebhookDto.KioskId);

        if (kiosk is null)
        {
            return new BaseResult<RegisterWebhookDto, WebhookDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Kiosk>(),
                Response = null,
                Request = registerWebhookDto
            };
        }

        var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == registerWebhookDto.KioskId
                            && x.WebhookType == registerWebhookDto.WebhookType);

        if (webhook is not null)
        {
            return new BaseResult<RegisterWebhookDto, WebhookDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = MessageUtil.AlreadyExists<Webhook>(),
                Response = null,
                Request = registerWebhookDto
            };
        }

        webhook = _mapper.Map<Webhook>(registerWebhookDto);
        await _unitOfWork.GetRepository<Webhook>().InsertAsync(webhook);

        await _unitOfWork.CommitAsync();
        var webhookDto = _mapper.Map<WebhookDto>(webhook);

        return new BaseResult<RegisterWebhookDto, WebhookDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<Webhook>(),
            StatusCode = StatusCodes.Status201Created,
            Response = webhookDto,
            Request = registerWebhookDto
        };
    }

    public async Task<BaseResult<string, WebhookDto>> UpdateWebhook(string webhookId,
        UpdateWebhookDto updateWebhookDto)
    {
        var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
            predicate: x => x.WebhookId == webhookId);

        if (webhook is null)
        {
            return new BaseResult<string, WebhookDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Webhook>(),
                Response = null,
                Request = webhookId
            };
        }

        webhook = _mapper.Map(updateWebhookDto, webhook);

        _unitOfWork.GetRepository<Webhook>().Update(webhook);
        await _unitOfWork.CommitAsync();

        var webhookDto = _mapper.Map<WebhookDto>(webhook);

        return new BaseResult<string, WebhookDto>()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status202Accepted,
            Message = MessageUtil.UpdateSuccess<Webhook>(),
            Response = webhookDto,
            Request = webhookId
        };
    }

    public Task<BaseResult<string, WebhookDto>> GetWebhook(string webhookId)
    {
        throw new NotImplementedException();
    }

    public Task<BaseResult<string, Paginate<WebhookDto>>> GetWebhooks(WebhookQueryDto webhookQueryDto)
    {
        throw new NotImplementedException();
    }
}