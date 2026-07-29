using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Services.CapRabbitMQ.Messages.Notification;
using Services.CapRabbitMQ.Topics;
using Services.SignalR.Services;
using Services.SignalR.Signal.Notification;
using Services.Utils;

namespace Services.CapRabbitMQ.Subscribers;

public class NotificationCapSubscriber : ICapSubscribe
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationCapSubscriber> _logger;
    private readonly WebAppSignalService _webAppSignalService;

    public NotificationCapSubscriber(
        ILoggerFactory loggerFactory,
        WebAppSignalService webAppSignalService, IUnitOfWork unitOfWork)
    {
        _webAppSignalService = webAppSignalService;
        _unitOfWork = unitOfWork;
        _logger = loggerFactory.CreateLogger<NotificationCapSubscriber>();
    }

    [CapSubscribe(NotificationCapTopic.NotificationForceLogout)]
    public async Task HandleNotificationBan(NotificationForceLogoutCapMessage message)
    {
        try
        {
            _logger.LogInformation("HandleNotificationBan: Notify ban user");
            await _webAppSignalService.NotifyBanAccountAsync(message.AccountId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandleNotificationBan");
            throw;
        }
    }

    [CapSubscribe(NotificationCapTopic.NotificationOrder)]
    public async Task HandleNotificationOrder(NotificationOrderCapMessage message)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .SingleOrDefaultAsync(
                predicate: x => x.OrderId == message.OrderId
            );

        if (order is null)
        {
            return;
        }

        var accounts = await _unitOfWork.GetRepository<Account>()
            .GetListAsync(predicate: x =>
                x.RoleName == nameof(ERoleName.Admin) || x.OrganizationId == order.OrganizationId
            );


        var notification = new Notification()
        {
            NotificationId = Guid.NewGuid().ToString(),
            Title = string.Empty,
            Message = string.Empty,
            ReferenceId = order.OrderId,
            ReferenceType = nameof(Order),
            Severity = nameof(ESeverity.Warning),
            NotificationType = message.NotificationType.ToString(),
            CreatedBy = message.CreatedBy,
            CreatedDate = DateTime.Now,
            NotificationRecipients = accounts.Select(x => new NotificationRecipient()
            {
                NotificationRecipientId = Guid.NewGuid().ToString(),
                AccountId = x.AccountId,
                CreatedDate = x.CreatedDate,
                AccountRole = x.RoleName,
                IsRead = false,
                IsDeleted = false,
                UpdatedDate = null,
                DeletedDate = null,
                ReadDate = null,
            }).ToList()
        };

        try
        {
            switch (message.NotificationType)
            {
                case ENotificationType.OrderExecuteFailed:
                {
                    var content = NotificationUtil.OrderExecuteFailed(order.OrderId, order.KioskId);
                    notification.Title = content.Title;
                    notification.Message = content.Message;

                    await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);
                    await _unitOfWork.CommitAsync();


                    var tasks = accounts.Select(account => _webAppSignalService.SendNotificationAsync(account.AccountId,
                            new NotificationSignal()
                            {
                                NotificationId = notification.NotificationId,
                                Title = content.Title,
                                Message = content.Message,
                                Severity = ESeverity.Warning,
                            }
                        )
                    );

                    await Task.WhenAll(tasks);

                    _logger.LogInformation("HandleNotificationOrder: OrderExecuteFailed");

                    break;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandleNotificationBan");
            throw;
        }
    }

    [CapSubscribe(NotificationCapTopic.NotificationKiosk)]
    public async Task HandleNotificationKiosk(NotificationKioskCapMessage message)
    {
        var kiosk = await _unitOfWork.GetRepository<Kiosk>()
            .SingleOrDefaultAsync(
                predicate: x => x.KioskId == message.KioskId,
                include: x => x.Include(x => x.Store)
            );

        if (kiosk is null)
        {
            return;
        }

        var accounts = await _unitOfWork.GetRepository<Account>()
            .GetListAsync(predicate: x =>
                x.RoleName == ERoleName.Admin.ToString() || x.OrganizationId == kiosk.Store!.OrganizationId
            );

        var notification = new Notification()
        {
            NotificationId = Guid.NewGuid().ToString(),
            Title = string.Empty,
            Message = string.Empty,
            ReferenceId = kiosk.KioskId,
            ReferenceType = nameof(Kiosk),
            Severity = nameof(ESeverity.Critical),
            NotificationType = message.NotificationType.ToString(),
            CreatedBy = message.CreatedBy,
            CreatedDate = DateTime.UtcNow,
            NotificationRecipients = accounts.Select(x => new NotificationRecipient()
            {
                NotificationRecipientId = Guid.NewGuid().ToString(),
                AccountId = x.AccountId,
                CreatedDate = x.CreatedDate,
                AccountRole = x.RoleName,
                IsRead = false,
                IsDeleted = false,
                UpdatedDate = null,
                DeletedDate = null,
                ReadDate = null,
            }).ToList()
        };

        try
        {
            switch (message.NotificationType)
            {
                case ENotificationType.KioskNotWorking:
                {
                    var content = NotificationUtil.KioskNotWorking(kiosk.KioskId);
                    notification.Title = content.Title;
                    notification.Message = content.Message;

                    await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);
                    await _unitOfWork.CommitAsync();


                    var tasks = accounts.Select(account => _webAppSignalService.SendNotificationAsync(account.AccountId,
                            new NotificationSignal()
                            {
                                NotificationId = notification.NotificationId,
                                Title = content.Title,
                                Message = content.Message,
                                Severity = Enum.Parse<ESeverity>(notification.Severity),
                                ReferenceId = notification.ReferenceId,
                                ReferenceType = notification.ReferenceType
                            }
                        )
                    );

                    await Task.WhenAll(tasks);

                    _logger.LogInformation("HandleNotificationKiosk: KioskNotWorking");

                    break;
                }

                case ENotificationType.KioskNotEnoughIngredient:
                {
                    var missingIngredient =
                        JsonConvert.DeserializeObject<List<IngredientHelper.MissingIngredientInfo>>(
                            message.Delivery?.ToString() ?? string.Empty);

                    var content = NotificationUtil.KioskNotEnoughIngredient(kiosk.KioskId,
                        missingIngredient ?? new List<IngredientHelper.MissingIngredientInfo>());
                    notification.Title = content.Title;
                    notification.Message = content.Message;
                    notification.Severity = nameof(ESeverity.Warning);

                    await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);
                    await _unitOfWork.CommitAsync();

                    var tasks = accounts.Select(account => _webAppSignalService.SendNotificationAsync(account.AccountId,
                            new NotificationSignal()
                            {
                                NotificationId = notification.NotificationId,
                                Title = content.Title,
                                Message = content.Message,
                                Severity = Enum.Parse<ESeverity>(notification.Severity),
                                ReferenceId = notification.ReferenceId,
                                ReferenceType = notification.ReferenceType
                            }
                        )
                    );

                    await Task.WhenAll(tasks);

                    _logger.LogInformation("HandleNotificationKiosk: KioskNotEnoughIngredient");

                    break;
                }

                case ENotificationType.KioskReceiveOrderFailed:
                {
                    var content = NotificationUtil.KioskReceiveOrderFailed(
                        kiosk.KioskId,
                        message.Delivery ?? string.Empty
                    );

                    notification.Title = content.Title;
                    notification.Message = content.Message;
                    notification.Severity = nameof(ESeverity.Critical);

                    await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);
                    await _unitOfWork.CommitAsync();

                    var tasks = accounts.Select(account => _webAppSignalService.SendNotificationAsync(account.AccountId,
                            new NotificationSignal()
                            {
                                NotificationId = notification.NotificationId,
                                Title = content.Title,
                                Message = content.Message,
                                Severity = Enum.Parse<ESeverity>(notification.Severity),
                                ReferenceId = notification.ReferenceId,
                                ReferenceType = notification.ReferenceType
                            }
                        )
                    );

                    await Task.WhenAll(tasks);

                    _logger.LogInformation("HandleNotificationKiosk: KioskNotEnoughIngredient");

                    break;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "HandleNotificationKiosk");
            throw;
        }
    }
}