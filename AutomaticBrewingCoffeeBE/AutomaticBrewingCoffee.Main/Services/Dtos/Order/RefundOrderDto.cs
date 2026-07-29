using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Order;

public class RefundOrderDto
{
    [StringLength(450)] public string? Content { get; set; }
    public decimal? RefundAmount { get; set; }
}