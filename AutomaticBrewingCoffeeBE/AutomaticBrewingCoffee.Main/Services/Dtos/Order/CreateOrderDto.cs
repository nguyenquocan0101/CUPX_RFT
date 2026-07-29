using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.OrderDetail;
using Services.Validations;

namespace Services.Dtos.Order;

public class CreateOrderDto
{
    public string KioskId { get; set; } = null!;
    public string Content { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string? DiscountCode { get; set; }

    [MatchEnum(typeof(EPaymentGateway))] public string PaymentGateway { get; set; } = EPaymentGateway.MPOS.ToString();

    public List<OrderDetailNestedDto> OrderDetails { get; set; } = [];
}