namespace Services.Dtos.Product;

public class ProductExecuteDto
{
    public string ProductId { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public decimal SellingPrice { get; set; } = 0;
}