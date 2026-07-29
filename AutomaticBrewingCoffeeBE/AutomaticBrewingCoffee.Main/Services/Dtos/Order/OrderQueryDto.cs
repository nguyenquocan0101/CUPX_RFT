using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Order;

public class OrderQueryDto : BaseQuery
{
    [MatchEnum(typeof(EOrderType))] public string? OrderType { get; set; }
    [MatchEnum(typeof(EPaymentGateway))] public string? PaymentGateway { get; set; }
    [MatchEnum(typeof(EOrderStatus))] public string? Status { get; set; }

    public string? OrganizationId { get; set; }
    
    public string? OrderCode { get; set; }

    public string? StoreId { get; set; }

    public string? KioskId { get; set; }
}