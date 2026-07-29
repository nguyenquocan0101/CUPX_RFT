using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Services.SignalR.Base;
using Services.SignalR.Signal.Notification;
using Services.Utils;

namespace Services.SignalR.Services;

public class WebAppSignalService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<WebAppSignalService> _logger;

    public WebAppSignalService(IHubContext<NotificationHub> hubContext, ILogger<WebAppSignalService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<bool> NotifyBanAccountAsync(string accountId)
    {
        try
        {
            _logger.LogInformation("NotifyBanAccountAsync called");
            await _hubContext.Clients.User(accountId)
                .SendAsync(SignalREvents.ForceLogout, MessageUtil.IsBan<Account>());
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> SendNotificationAsync(string accountId, NotificationSignal notification)
    {
        try
        {
            _logger.LogInformation("SendNotificationAsync called");
            await _hubContext.Clients.User(accountId)
                .SendAsync(SignalREvents.ReceiveNotification, notification);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}