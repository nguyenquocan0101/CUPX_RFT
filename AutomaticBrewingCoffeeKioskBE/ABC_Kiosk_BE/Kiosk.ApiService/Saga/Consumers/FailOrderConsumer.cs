using Kiosk.ApiService.Saga.Contracts;
using MassTransit;

namespace Kiosk.ApiService.Saga.Consumers
{
    public class FailOrderConsumer : IConsumer<FailOrder>
    {
        public Task Consume(ConsumeContext<FailOrder> context)
        {
            var orderCompletedEvent = new OrderFailed
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
                OrderFailedAt = DateTime.UtcNow
            };

            Console.WriteLine($"Published: OrderCompleted - CorrelationId: {orderCompletedEvent.CorrelationId}, OrderId: {orderCompletedEvent.OrderId}");
            return context.RespondAsync(orderCompletedEvent);
        }
    }
}
