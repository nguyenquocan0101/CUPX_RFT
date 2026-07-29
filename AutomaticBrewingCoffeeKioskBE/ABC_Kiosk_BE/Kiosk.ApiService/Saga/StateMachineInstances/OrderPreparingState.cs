using MassTransit;
using Services.Dtos.Order;

namespace Kiosk.ApiService.Saga.StateMachineInstances
{
    public class OrderPreparingState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = default!;
        public CreateLocalOrderDto Request { get; set; } = default!;
        public Uri? ResponseAddress { get; set; }
        public Guid? RequestId { get; set; } = default!;
        public string OrderId { get; set; } = default!;
        public decimal Discount { get; set; } = 0;
        public decimal FinalAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public string? OrderStatus { get; set; }
        public string PaymentGateway { get; set; } = default!;
        public string PaymentUrl { get; set; } = default!;
        public string PaymentQr { get; set; } = default!;
        public ICollection<LocalOrderDetailDto> OrderDetails { get; set; }
        public bool IsCompleted { get; set; }

    }
}