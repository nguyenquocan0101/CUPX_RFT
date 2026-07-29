using MassTransit;

namespace Kiosk.ApiService.Saga.Contracts
{
    public record CreatePayment(Guid CorrelationId, string OrderId, decimal FinalAmount, string PaymentGateway) : CorrelatedBy<Guid>;

    public record PaymentCreated(Guid CorrelationId) : CorrelatedBy<Guid>;
}
