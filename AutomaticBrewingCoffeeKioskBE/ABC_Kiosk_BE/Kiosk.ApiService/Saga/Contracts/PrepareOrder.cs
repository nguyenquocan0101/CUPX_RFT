using MassTransit;
using Services.Dtos.Order;

namespace Kiosk.ApiService.Saga.Contracts
{
    public record PrepareOrder : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public CreateLocalOrderDto Request { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
    }

    public record OrderPrepared(
     Guid CorrelationId,
     string OrderId,
     DateTime? OrderPrepareddAt,
     string PaymentUrl,
     string PaymentQr,
     ICollection<LocalOrderDetailDto> OrderDetails
    ) : CorrelatedBy<Guid>;


}
