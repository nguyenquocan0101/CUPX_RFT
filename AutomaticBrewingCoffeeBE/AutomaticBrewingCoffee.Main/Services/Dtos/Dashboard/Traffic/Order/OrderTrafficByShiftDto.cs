namespace Services.Dtos.Dashboard.Traffic.Order;

public class OrderTrafficByShiftDto
{
    public int Dow { get; set; }
    public string DowLabel { get; set; } = default!;
    public string Shift { get; set; } = default!;
    public int Count { get; set; }
}