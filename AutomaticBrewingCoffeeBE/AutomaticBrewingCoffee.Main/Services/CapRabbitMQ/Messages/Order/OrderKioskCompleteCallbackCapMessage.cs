namespace Services.CapRabbitMQ.Messages.Order;

public class OrderKioskCompleteCallbackCapMessage
{
    public string OrderId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public List<string> FinishedProductIdList { get; set; } = null!;
}