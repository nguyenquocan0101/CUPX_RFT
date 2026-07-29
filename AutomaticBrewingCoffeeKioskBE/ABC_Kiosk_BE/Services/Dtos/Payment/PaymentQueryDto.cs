
using Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Payment;

public class PaymentQueryDto : BaseQuery
{
    public PaymentStatus Status { get; set; }
}