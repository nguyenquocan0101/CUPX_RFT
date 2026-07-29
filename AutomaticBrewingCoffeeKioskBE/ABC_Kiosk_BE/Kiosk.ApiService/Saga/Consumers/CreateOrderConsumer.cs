using Kiosk.ApiService.Saga.Contracts;
using MassTransit;
using Services.Interfaces;

namespace Kiosk.ApiService.Saga.Consumers
{
    public class CreateOrderConsumer(IOrderService orderService) : IConsumer<CreateOrder>
    {
        public async Task Consume(ConsumeContext<CreateOrder> context)
        {
                var orderDto = await orderService.CreateOrder(context.Message.OrderData);

                var orderCreated = new OrderCreated(
                    context.Message.CorrelationId,
                    orderDto.OrderId,
                    orderDto.Discount ?? 0,
                    orderDto.FinalAmount ?? 0,
                    orderDto.TotalAmount ?? 0,
                    orderDto.Status,
                    orderDto.PaymentUrl,
                    orderDto.PaymentQr,
                    orderDto.OrderDetails
                );
                Console.WriteLine($"Published: OrderCreated - CorrelationId: {orderCreated.CorrelationId}");
                await context.RespondAsync(orderCreated);
        }
    }
}
