using Kiosk.ApiService.Saga.Contracts;
using MassTransit;

namespace Kiosk.ApiService.Saga.Consumers;

public class CompleteOrderConsumer : IConsumer<CompleteOrder>
{
    public Task Consume(ConsumeContext<CompleteOrder> context)
    {
        var orderCompletedEvent = new OrderCompleted
        {
            CorrelationId = context.Message.CorrelationId,
            OrderId = context.Message.OrderId,
            OrderCompletedAt = DateTime.UtcNow
        };

        Console.WriteLine($"Published: OrderCompleted - CorrelationId: {orderCompletedEvent.CorrelationId}, OrderId: {orderCompletedEvent.OrderId}");
        return context.RespondAsync(orderCompletedEvent);
    }
}