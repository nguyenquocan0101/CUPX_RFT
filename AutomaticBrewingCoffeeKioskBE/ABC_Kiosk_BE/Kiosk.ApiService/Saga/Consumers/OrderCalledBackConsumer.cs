using Domain.Models;
using Kiosk.ApiService.Saga.Contracts;
using MassTransit;
using Repositories.Interfaces;

namespace Kiosk.ApiService.Saga.Consumers
{
    public class OrderCalledBackConsumer(IPublishEndpoint publishEndpoint, IUnitOfWork unitOfWork) : IConsumer<OderCalledBack>
    {
        public async Task Consume(ConsumeContext<OderCalledBack> context)
        {
            var orderId = context.Message.OrderId;
            Console.WriteLine($"Confirm Order {orderId} paid. Move it pending. CorrelationId: {context.CorrelationId}");
            var order = await unitOfWork.GetRepository<LocalOrder>().SingleOrDefaultAsync(predicate: x => x.OrderId.Equals(orderId));
            //incase not found -> do nothing then stop
            //TODO: send sse to client to notify that fail
            if (order == null) return;

            //Update order based on status 
            order.Status = context.Message.Status;
            unitOfWork.GetRepository<LocalOrder>().Update(order);

            //Set Order to Pending Queue
            await publishEndpoint.Publish(new QueueOrder(context.Message.CorrelationId, orderId));
        }
    }


    public class FaultOrderPaidHandler : IConsumer<Fault<OderCalledBack>>
    {
        public Task Consume(ConsumeContext<Fault<OderCalledBack>> context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Confirm Order {context.Message.Message.OrderId} fail. Wait for other run. CorrelationId: {context.CorrelationId}");
            Console.ResetColor();

            //TODO: send sse to client notify that it is failed to do service

            return Task.CompletedTask;
        }
    }
}
