namespace Services.Dtos.Order;

public class OrderExecuteDto
{
    public string OrderId { get; set; } = null!;
    public int? Side { get; set; } = null!;
    
    public List<OrderProductDto> Products { get; set; } = new();
}