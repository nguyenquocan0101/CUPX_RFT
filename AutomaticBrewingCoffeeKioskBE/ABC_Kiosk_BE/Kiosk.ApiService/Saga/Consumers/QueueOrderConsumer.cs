using Kiosk.ApiService.Saga.Contracts;
using MassTransit;

namespace Kiosk.ApiService.Saga.Consumers
{
    public class QueueOrderConsumer(IRequestClient<WorkflowInit> client) : IConsumer<QueueOrder>
    {
        public async Task Consume(ConsumeContext<QueueOrder> context)
        {
            Console.WriteLine($"Order Queue {context.Message.OrderId}");
            var response = await client.GetResponse<WorkflowCompleted>(new WorkflowInit()
            {
                CorrelationId = context.Message.CorrelationId,
                OrderId = context.Message.OrderId,
            });
            await Task.Delay(10000);
        }
    }
}
