using System.Security.Claims;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Services.SignalR;

[Authorize]
public sealed class NotificationHub : Hub
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(IUnitOfWork unitOfWork, ILogger<NotificationHub> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation(
            $"New client connected: {Context.ConnectionId} - {Context.User!.FindFirst(ClaimTypes.Email)}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            $"A client disconnected: {Context.ConnectionId} - {Context.User!.FindFirst(ClaimTypes.Email)}");
        return base.OnDisconnectedAsync(exception);
    }
}