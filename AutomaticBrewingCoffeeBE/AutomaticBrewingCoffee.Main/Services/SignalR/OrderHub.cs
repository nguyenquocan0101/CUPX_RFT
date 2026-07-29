using System.Collections.Concurrent;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Services.Utils;

namespace Services.SignalR;

public class OrderHub(ILoggerFactory loggerFactory, IUnitOfWork unitOfWork) : Hub
{
    public static readonly ConcurrentDictionary<string, string> ClientIdToConnectionId = new();
    private readonly ILogger<OrderHub> _logger = loggerFactory.CreateLogger<OrderHub>();

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"OrderHub.OnConnectedAsync");

        var isAuthenticate = await InvokeAuthentication();

        if (!isAuthenticate)
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"OrderHub.OnDisconnectedAsync");

        RevokeAuthentication();

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> InvokeAuthentication()
    {
        var httpContext = Context.GetHttpContext();

        if (httpContext is null)
        {
            _logger.LogInformation("OrderHub: HttpContext is null");
            return false;
        }

        var apiKey = httpContext.Request.Query["apiKey"].ToString();
        var kioskId = httpContext.Request.Query["kioskId"].ToString();
        var clientId = httpContext.Request.Query["clientId"].ToString();

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogInformation($"OrderHub.InvokeAuthentication: ApiKey is null");
            return false;
        }

        if (string.IsNullOrEmpty(kioskId))
        {
            _logger.LogInformation($"OrderHub.InvokeAuthentication: KioskId is null");
            return false;
        }

        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogInformation($"OrderHub.InvokeAuthentication: ClientId is null");
            return false;
        }

        var kiosk = await unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kioskId
        );

        if (kiosk is null)
        {
            _logger.LogInformation($"OrderHub.InvokeAuthentication: Kiosk is null");
            return false;
        }

        if (!ApiKeyUtil.Encrypt(apiKey).Equals(kiosk.ApiKey))
        {
            _logger.LogInformation($"OrderHub.InvokeAuthentication: ApiKey is invalid");
            return false;
        }


        _logger.LogInformation($"OrderHub.InvokeAuthentication: ApiKey = {apiKey}");
        _logger.LogInformation($"OrderHub.InvokeAuthentication: ClientId = {clientId}");
        _logger.LogInformation($"OrderHub.InvokeAuthentication: KioskId = {kiosk.KioskId}");

        ClientIdToConnectionId[clientId] = Context.ConnectionId;

        return true;
    }

    private void RevokeAuthentication()
    {
        var clientId = Context.GetHttpContext()?.Request.Query["client_id"].ToString();

        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogInformation($"OrderHub.RevokeAuthentication: ClientId is null");
            return;
        }

        ClientIdToConnectionId.TryRemove(clientId, out _);
        _logger.LogInformation($"OrderHub.RevokeAuthentication: ClientId = {clientId}");
    }
}