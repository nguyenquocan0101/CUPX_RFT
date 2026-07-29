namespace Services.CapRabbitMQ.Messages.Order;

public class OrderKioskFailCallbackCapMessage
{
    public string OrderId { get; set; } = null!;
    public string Status { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    public List<string> FinishedProductIds { get; set; } = new List<string>();
    public List<string> FailedProductIds { get; set; } = new List<string>();
    public List<string> PreparingProductIds { get; set; } = new List<string>();
}