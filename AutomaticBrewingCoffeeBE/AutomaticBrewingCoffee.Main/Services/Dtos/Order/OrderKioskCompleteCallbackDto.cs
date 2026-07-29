namespace Services.Dtos.Order;

public class OrderKioskCompleteCallbackDto
{
    public string OrderId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public List<string> FinishedProductIdList { get; set; } = null!;
}