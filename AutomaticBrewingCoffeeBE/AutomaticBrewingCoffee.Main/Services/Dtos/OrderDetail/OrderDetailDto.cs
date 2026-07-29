using Services.Dtos.ProductAttribute;

namespace Services.Dtos.OrderDetail;

public class OrderDetailDto
{
    public string OrderDetailId { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal SellingPrice { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public int Quantity { get; set; }
    
    public List<ProductAttributeSelectDto>? ProductAttributes { get; set; }
}