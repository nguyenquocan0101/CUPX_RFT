using Kiosk.ApiService.Saga.Contracts;
using MassTransit;

namespace Kiosk.ApiService.Saga.Consumers
{
    public class CreatePaymentConsumer : IConsumer<CreatePayment>
    {

        public async Task Consume(ConsumeContext<CreatePayment> context)
        {
            //! DO NOTHING 
            await context.RespondAsync(new PaymentCreated(context.Message.CorrelationId));
        }
    }
}
