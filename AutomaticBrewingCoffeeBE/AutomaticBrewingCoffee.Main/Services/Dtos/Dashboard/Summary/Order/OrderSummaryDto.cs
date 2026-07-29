using Services.Dtos.Order;

namespace Services.Dtos.Dashboard.Summary.Order;

public class OrderSummaryDto
{
    public int Total { get; set; } = 0;
    public int Pending { get; set; } = 0;
    public int Preparing { get; set; } = 0;
    public int Completed { get; set; } = 0;
    public int Cancelled { get; set; } = 0;
    public int Failed { get; set; } = 0;

    public List<OrderInsideDto>? RecentOrders { get; set; }
}