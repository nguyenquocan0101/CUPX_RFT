using Domain.Enums;
using Domain.Models;
using Kiosk.ApiService.Saga.Contracts;
using MassTransit;
using Services.WorkflowEngine;

namespace Kiosk.ApiService.Saga.Consumers
{
    public class DoWorkflowConsumer(WorkflowExecutor executor, IBus bus) : IConsumer<DoWorkflow>
    {
        public async Task Consume(ConsumeContext<DoWorkflow> context)
        {
            #region Do Workflow
            var blackCoffeeWorkflowId = "123";

            var workflow = new Workflow
            {
                WorkflowId = blackCoffeeWorkflowId,
                ProductId = "1",
                Name = "BlackCoffeeFlow",
                Description = "Make a cup of black coffee",
                Type = WorkflowType.Activity,
                Steps = new List<Step>
                {
                    new Step
                    {
                        //StepId = "1",
                        //WorkflowId = blackCoffeeWorkflowId,
                        //Name = "Drop Cup",
                        //Type = StepType.DropCupCommand,
                        //Sequence = 1,
                        //MaxRetries = 3,
                        //Parameters = "{}"
                    },
                    new Step
                    {
                        //StepId = "2",
                        //WorkflowId = blackCoffeeWorkflowId,
                        //Name = "Move Arm to Cup",
                        //Type = StepType.MoveArmCommand,
                        //Sequence = 2,
                        //MaxRetries = 3,
                        //Parameters = "{\"target\": \"CupPosition\"}"
                    },
                    new Step
                    {
                        //StepId = "3",
                        //WorkflowId = blackCoffeeWorkflowId,
                        //Name = "Take Ice",
                        //Type = StepType.TakeIceCommand,
                        //Sequence = 3,
                        //MaxRetries = 3,
                        //Parameters = "{\"amount\": \"less\"}"
                    },
                    new Step
                    {
                        //StepId = "4",
                        //WorkflowId = blackCoffeeWorkflowId,
                        //Name = "Move Arm to Coffee",
                        //Type = StepType.MoveArmCommand,
                        //Sequence = 4,
                        //MaxRetries = 3,
                        //Parameters = "{\"target\": \"CoffeeMachine\"}"
                    },
                    new Step
                    {
                        //StepId = "5",
                        //WorkflowId = blackCoffeeWorkflowId,
                        //Name = "Make Coffee",
                        //Type = StepType.MakeDrinkCommand,
                        //Sequence = 5,
                        //MaxRetries = 3,
                        //Parameters = "{\"type\": \"Black\"}"
                    },
                    new Step
                    {
                        //StepId = "6",
                        //WorkflowId = blackCoffeeWorkflowId,
                        //Name = "Move Arm to Drop",
                        //Type = StepType.MoveArmCommand,
                        //Sequence = 6,
                        //MaxRetries = 3,
                        //Parameters = "{\"target\": \"DropZone\"}"
                    },
                    new Step
                    {
                        //StepId = "7",
                        //WorkflowId = blackCoffeeWorkflowId,
                        //Name = "Release Cup",
                        //Type = StepType.OpenGripperCommand,
                        //Sequence = 7,
                        //MaxRetries = 3,
                        //Parameters = "{}"
                    }
                }
            };
            await executor.Execute(workflow);
            #endregion

            var demo = context.ResponseAddress;

            var workflowDoneEvent = new WorkflowDone()
            {
                CorrelationId = context.Message.CorrelationId,
                WorkflowDoneAt = DateTime.UtcNow,
            };
            await bus.Publish(workflowDoneEvent);
            //await context.RespondAsync(workflowDoneEvent);
            Console.WriteLine($"Published: WorkflowDone - CorrelationId: {workflowDoneEvent.CorrelationId}");
        }
    }

}
