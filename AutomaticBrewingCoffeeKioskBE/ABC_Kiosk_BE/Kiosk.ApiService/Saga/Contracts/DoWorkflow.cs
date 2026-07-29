using MassTransit;

namespace Kiosk.ApiService.Saga.Contracts
{
    public record WorkflowInit : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;

    }
    public record DoWorkflow : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
      
    }

    public record WorkflowDone : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public DateTime? WorkflowDoneAt { get; set; }
    }

    public record WorkflowCompleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? WorkflowCompletedAt { get; set; }
        public bool IsSuccess { get; set; }

    }
    public record WorkflowFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public DateTime? WorkflowFailedAt { get; set; }
    }
}
