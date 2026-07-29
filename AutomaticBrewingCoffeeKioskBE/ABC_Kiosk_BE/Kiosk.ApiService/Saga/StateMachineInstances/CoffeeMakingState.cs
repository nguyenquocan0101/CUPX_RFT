using MassTransit;
using System.Collections.Generic; // Thêm using này
using System; // Thêm using này

namespace Kiosk.ApiService.Saga.StateMachineInstances
{
    public class CoffeeMakingState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = default!;
        public Uri? ResponseAddress { get; set; }
        public string OrderId { get; set; } = default!;
        public string PaymentId { get; set; } = default!;
        public Guid? RequestId { get; set; } = default!;
        public DateTime? OrderProcessedAt { get; set; }
        public DateTime? WorkflowDoneAt { get; set; }
        public DateTime? OrderUpdatedAt { get; set; }
        public bool IsSuccess { get; set; }

        //public List<string> TargetMovementHistory { get; set; } = [];
        //public bool ArmIdle { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string? FailureReason { get; set; }

    }
}