using MassTransit;

namespace Kiosk.ApiService.Saga.Contracts
{
    public record CompleteOrder(Guid CorrelationId, string OrderId) : CorrelatedBy<Guid>;

    public record OrderCompleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderCompletedAt { get; set; }
    }


    public record FailOrder(Guid CorrelationId, string OrderId) : CorrelatedBy<Guid>;

    public record OrderFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderFailedAt { get; set; }
    }
}
