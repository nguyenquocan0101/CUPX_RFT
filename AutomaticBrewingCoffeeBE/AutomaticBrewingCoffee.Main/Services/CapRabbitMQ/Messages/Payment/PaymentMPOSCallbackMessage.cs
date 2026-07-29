namespace Services.CapRabbitMQ.Messages.Payment;

public class PaymentMPOSCallbackMessage
{
    public string ServiceName { get; set; }

    public long TransStatus { get; set; }

    public string TransCode { get; set; }

    public long TransDate { get; set; }

    public long TransAmount { get; set; }

    public string IssuerCode { get; set; }

    public string Muid { get; set; }

    public string OrderId { get; set; }

    public string PosId { get; set; }

    public string? TranStatusEnum { get; set; }
}