using Microsoft.EntityFrameworkCore;

namespace Services.SignalR.Signal.Payment;

public class PaymentSignal
{
    public string PaymentId { get; set; } = null!;

    public string? OrderId { get; set; }

    [Precision(18, 2)] public decimal? PaidAmount { get; set; }

    public string? PaymentStatus { get; set; }
    
    public string? OrderStatus { get; set; }
}