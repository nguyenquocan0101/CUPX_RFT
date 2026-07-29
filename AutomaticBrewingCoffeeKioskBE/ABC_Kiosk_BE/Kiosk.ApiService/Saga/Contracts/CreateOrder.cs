using MassTransit;
using Services.Dtos.Order;

namespace Kiosk.ApiService.Saga.Contracts
{
    public record CreateOrder(
        Guid CorrelationId,
        CreateLocalOrderDto OrderData) : CorrelatedBy<Guid>;
       
    

    public record OrderCreated
        (
        Guid CorrelationId,
            string OrderId,
            decimal Discount,
            decimal FinalAmount,
            decimal TotalAmount,
            string? Status,
            string? paymentUrl,
            string? paymentQr,
            ICollection<LocalOrderDetailDto> orderDetails
        )
        : CorrelatedBy<Guid>;
}
