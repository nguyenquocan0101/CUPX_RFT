using Kiosk.ApiService.Saga.StateMachineInstances;
using MassTransit;
using Kiosk.ApiService.Saga.Contracts;
using Serilog;

namespace Kiosk.ApiService.Saga.StateMachines
{
    public class CoffeeMakingStateMachine : MassTransitStateMachine<CoffeeMakingState>
    {
        public State WaitForWorkflowDone { get; }
        public State CompleteOrder { get; }
        public State FailOrder { get; }
        public State Idle { get; }
        public State Completed { get; }

        // Workflow events
        public Event<WorkflowInit> Init { get; }
        public Event<WorkflowDone> WorflowDone { get; }
        public Event<WorkflowFailed> WorkflowFail { get; }

        public Request<CoffeeMakingState, CompleteOrder, OrderCompleted> CompleteOrderRequest { get; init; }
        public Request<CoffeeMakingState, FailOrder, OrderFailed> FailOrderRequest { get; init; }

        public CoffeeMakingStateMachine()
        {
            // Define current saga state
            InstanceState(x => x.CurrentState);

            // Define Events
            Event(() => Init, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => WorflowDone, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => WorkflowFail, x => x.CorrelateById(context => context.Message.CorrelationId));

            // Define Request
            Request(() => CompleteOrderRequest, r => r.Timeout = TimeSpan.Zero);
            Request(() => FailOrderRequest, r => r.Timeout = TimeSpan.Zero);

            Initially(
                When(Init)
                    .Then(context =>
                    {
                        // Set saga context information
                        Log.Information("Set Saga Content Hit");
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        context.Saga.ResponseAddress = context.ResponseAddress;
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.PaymentId = context.Message.PaymentId;
                        context.Saga.RequestId = context.RequestId;
                        context.Saga.OrderProcessedAt = DateTime.UtcNow;

                    })
                    .Publish(context => new DoWorkflow
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        OrderId = context.Saga.OrderId,
                        PaymentId = context.Saga.PaymentId,
                    })
                    .TransitionTo(WaitForWorkflowDone)
            );

            During(WaitForWorkflowDone,
                When(WorflowDone)
                    .Then(context =>
                    {
                        context.Saga.WorkflowDoneAt = context.Message.WorkflowDoneAt;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Workflow Done Event Hit");
                        Console.ResetColor();
                    })
                    .TransitionTo(CompleteOrder)
                    .Request(CompleteOrderRequest, (context) => new CompleteOrder(context.Saga.CorrelationId, context.Saga.OrderId)),

                When(WorkflowFail)
                    .Then(context =>
                    {
                        context.Saga.WorkflowDoneAt = context.Message.WorkflowFailedAt;
                        context.Saga.FailureReason += "Workflow Run Failed,";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("⚠Workflow Failed Event Hit");
                        Console.ResetColor();
                    })
                    .TransitionTo(FailOrder)
                    .Request(FailOrderRequest, (context) => new FailOrder(context.Saga.CorrelationId, context.Saga.OrderId))
            );

            // Update Order when workflow success
            During(CompleteOrder,
                When(CompleteOrderRequest.Completed)
                    .Then(context =>
                    {
                        context.Saga.OrderUpdatedAt = context.Message.OrderCompletedAt;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("CompleteOrder Event Hit");
                        Console.ResetColor();
                    })
                    .TransitionTo(Completed),

                When(CompleteOrderRequest.Faulted)
                    .Then(context =>
                    {
                        context.Saga.IsSuccess = false;
                        context.Saga.FailureReason += "Complete Order Failed,";
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Fault to complete Order");
                        Console.ResetColor();
                    })
                    .TransitionTo(Completed)
            );

            During(FailOrder,
                When(FailOrderRequest.Completed)
                    .Then(context =>
                    {
                        context.Saga.IsSuccess = false;
                        context.Saga.OrderUpdatedAt = context.Message.OrderFailedAt;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("FailOrder Event Hit");
                        Console.ResetColor();
                    })
                    .TransitionTo(Completed),

                When(FailOrderRequest.Faulted)
                    .Then(context =>
                    {
                        context.Saga.FailureReason += "Set Order Failed Status Failed,";
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Fault to update fail Order");
                        Console.ResetColor();
                    })
                    .TransitionTo(Completed)
            );

            WhenEnter(Completed, x => x
                .Then(context =>
                {
                    context.Saga.IsCompleted = true;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Entered COMPLETED stage. Sending final workflow result.");
                    Console.ResetColor();
                })
                .Send(context => context.Saga.ResponseAddress!,
                        context => new WorkflowCompleted
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            OrderId = context.Saga.OrderId,
                            PaymentId = context.Saga.PaymentId,
                            IsSuccess = context.Saga.IsSuccess,
                            IsCompleted = context.Saga.IsCompleted,
                            WorkflowCompletedAt = DateTime.UtcNow,
                        },
                        (context, sendCtx) =>
                        {
                            sendCtx.RequestId = context.Saga.RequestId;
                        })
                .TransitionTo(Idle)
            );
            WhenEnter(Idle, x => x
                .Then(context =>
                {
                    context.Saga.IsCompleted = true;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Move every things to Idle state");
                    Console.ResetColor();
                })
                .Finalize()
            );
        }
    }
}
