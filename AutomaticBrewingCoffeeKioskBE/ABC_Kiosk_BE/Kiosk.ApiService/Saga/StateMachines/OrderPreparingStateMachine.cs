using Kiosk.ApiService.Saga.Contracts;
using Kiosk.ApiService.Saga.StateMachineInstances;
using MassTransit;
using Serilog;

namespace Kiosk.ApiService.Saga.StateMachines
{
    public class OrderPreparingStateMachine : MassTransitStateMachine<OrderPreparingState>
    {
        public State Idle { get; private set; } = default!;
        public State CreateOrder { get; private set; } = default!;
        public State CreatePayment { get; private set; } = default!;
        public State CompletedState { get; private set; } = default!;
        public Event<PrepareOrder> Initial { get; private set; } = default!;
        public Request<OrderPreparingState, CreateOrder, OrderCreated> CreateOrderRequest { get; init; } = default!;
        public Request<OrderPreparingState, CreatePayment, PaymentCreated> CreatePaymentRequest { get; init; } = default!;

        public OrderPreparingStateMachine()
        {
            InstanceState(x => x.CurrentState);

            // Define Events
            Event(() => Initial, x => x.CorrelateById(context => context.Message.CorrelationId));

            // Define Request
            Request(() => CreateOrderRequest, r => r.Timeout = TimeSpan.FromSeconds(0));
            Request(() => CreatePaymentRequest, r => r.Timeout = TimeSpan.FromSeconds(0));

            Initially(
                When(Initial)
                    .Then(context =>
                    {
                        // Set saga context information
                        Log.Information("Hit PrepareOrder event. Set data for Saga Data");
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        context.Saga.ResponseAddress = context.ResponseAddress;
                        context.Saga.RequestId = context.RequestId;
                        context.Saga.Request = context.Message.Request;

                    })
                    .Request(CreateOrderRequest, (context) => new CreateOrder(context.Saga.CorrelationId, context.Saga.Request))
                    .TransitionTo(CreateOrder)
            );

            During(CreateOrder,
                When(CreateOrderRequest.Completed)
                    .Then(context =>
                    {
                        Log.Information("Hit OrderCreated event.Set data for Saga Data");
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.Discount = context.Message.Discount;
                        context.Saga.FinalAmount = context.Message.FinalAmount;
                        context.Saga.TotalAmount = context.Message.TotalAmount;
                        context.Saga.OrderStatus = context.Message.Status;
                        context.Saga.PaymentQr = context.Message.paymentQr;
                        context.Saga.PaymentUrl = context.Message.paymentUrl;
                        context.Saga.OrderDetails = context.Message.orderDetails;
                    })
                    .TransitionTo(CreatePayment)
                    .Request(CreatePaymentRequest, context => new CreatePayment(
                        context.Saga.CorrelationId,
                        context.Saga.OrderId,
                        context.Saga.FinalAmount,
                        context.Saga.PaymentGateway
                    ))
                    );

            During(CreatePayment,
                When(CreatePaymentRequest.Completed)
                    .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                    .Send(context => context.Saga.ResponseAddress!,
                        context => new OrderPrepared(
                            context.Saga.CorrelationId,
                            context.Saga.OrderId,
                            null,
                            context.Saga.PaymentUrl,
                            context.Saga.PaymentQr,
                            context.Saga.OrderDetails),
                        (context, sendCtx) => sendCtx.RequestId = context.Saga.RequestId)
                .TransitionTo(CompletedState)
            );
            WhenEnter(CompletedState, x => x
               .Then(context => Log.Information("Prepare order successfully!"))
               .Finalize()
            );

        }

    }
}
