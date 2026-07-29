using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Services.Dtos.ProductAttribute;
using Services.Validations;

namespace Services.Dtos.OrderDetail;

public class OrderDetailNestedDto
{
    [Required] [StringLength(50)] public string ProductId { get; set; } = null!;

    [StringLength(100)] public string? ProductName { get; set; }

    [StringLength(300)] public string? ProductDescription { get; set; }

    [Required] [GreaterThan(0)] public int Quantity { get; set; }
    [GreaterThan(0)] [Required] public decimal SellingPrice { get; set; }

    public List<ProductAttributeSelectDto>? ProductAttributes { get; set; }

    [JsonIgnore] public decimal TotalAmount => SellingPrice * Quantity;

    public void Normalize()
    {
        if (ProductAttributes != null)
        {
            ProductAttributes = ProductAttributes
                .GroupBy(x => new { x.ProductAttributeId, x.AttributeOptionId })
                .Select(g => g.Last())
                .ToList();
        }
    }
}