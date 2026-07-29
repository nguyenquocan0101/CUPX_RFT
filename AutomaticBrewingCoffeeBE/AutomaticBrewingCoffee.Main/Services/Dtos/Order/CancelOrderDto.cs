namespace Services.Dtos.Order;

public class CancelOrderDto
{
    public string OrderId { get; set; } = null!;

    public string KioskId { get; set; } = null!;

    public string ClientId { get; set; } = null!;
}