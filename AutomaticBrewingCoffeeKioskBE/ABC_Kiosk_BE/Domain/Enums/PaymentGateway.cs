using System.ComponentModel.DataAnnotations;

namespace Domain.Enums;

public enum PaymentGateway
{
    //[Display(Name = "VNPay")] VNPay = 0,

    //[Display(Name = "MoMo")] MoMo = 1
    [Display(Name = "MPOS")] MPOS = 1

}