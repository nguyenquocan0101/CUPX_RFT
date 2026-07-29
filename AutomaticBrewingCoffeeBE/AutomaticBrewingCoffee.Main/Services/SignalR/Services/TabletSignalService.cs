using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Services.SignalR.Base;
using Services.SignalR.Signal.Order;
using Services.SignalR.Signal.Payment;

namespace Services.SignalR.Services;

public class TabletSignalService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<TabletSignalService> _logger;

    public TabletSignalService(IHubContext<OrderHub> hubContext, ILogger<TabletSignalService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Gửi trạng thái thanh toán về tablet qua SignalR
    /// </summary>
    public async Task<bool> NotifyPaymentAsync(string clientId, PaymentSignal paymentSignal)
    {
        if (OrderHub.ClientIdToConnectionId.TryGetValue(clientId, out var connectionId))
        {
            await _hubContext.Clients.Client(connectionId)
                .SendAsync(SignalREvents.ReceiveTrans, paymentSignal);

            _logger.LogInformation("TabletSignalService.NotifyPayment: {ClientId} | {Status}", clientId,
                paymentSignal.PaymentStatus);
            return true;
        }

        _logger.LogError("TabletSignalService.NotifyPayment: ClientId not found: {ClientId}", clientId);
        return false;
    }

    /// <summary>
    /// Gửi trạng thái đơn hàng về tablet qua SignalR
    /// </summary>
    public async Task<bool> NotifyOrderAsync(string clientId, OrderSignal orderSignal)
    {
        if (OrderHub.ClientIdToConnectionId.TryGetValue(clientId, out var connectionId))
        {
            await _hubContext.Clients.Client(connectionId)
                .SendAsync(SignalREvents.ReceiveOrderState, orderSignal);

            _logger.LogInformation("TabletSignalService.NotifyOrder: {ClientId} | {Status}", clientId,
                orderSignal.OrderStatus);
            return true;
        }

        _logger.LogError("TabletSignalService.NotifyOrder: ClientId not found: {ClientId}", clientId);
        return false;
    }
}