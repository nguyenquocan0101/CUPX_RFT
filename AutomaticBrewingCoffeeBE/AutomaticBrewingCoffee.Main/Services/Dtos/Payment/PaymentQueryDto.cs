using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Payment;

public class PaymentQueryDto : BaseQuery
{
    [MatchEnum(typeof(EPaymentStatus))] public string? Status { get; set; }
}