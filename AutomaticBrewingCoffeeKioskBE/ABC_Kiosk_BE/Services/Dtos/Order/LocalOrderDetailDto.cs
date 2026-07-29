using System.ComponentModel.DataAnnotations;
using Services.Validations;

namespace Services.Dtos.Order;

public class LocalOrderDetailDto
{

    public string? ProductName { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal SellingPrice { get; set; }

    public string? ProductDescription { get; set; }

    public int Quantity { get; set; }
}