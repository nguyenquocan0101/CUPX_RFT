namespace Services.Dtos.Order;

public class OrderProductDto
{
    public string ProductId { get; set; } = null!;
    public List<OrderOptionDto> Options { get; set; } = new();
}