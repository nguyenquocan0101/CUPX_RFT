using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Order;

public class CreateLocalOrderDto
{
    public PaymentGateway PaymentGateway { get; set; }
    //public decimal? Discount { get; set; }
    //public decimal FeeAmount { get; set; } = 0m;
    //[StringLength(50)] public string? FeeDescription { get; set; }
    //public OrderType OrderType { get; set; }
    public List<CreateLocalOrderDetailDto> CreateLocalOrderDetails { get; set; }
}