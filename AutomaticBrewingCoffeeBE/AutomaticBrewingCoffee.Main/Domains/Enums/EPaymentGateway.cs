using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EPaymentGateway
{
    [Display(Name = "VNPay")] VNPay = 0,

    [Display(Name = "RESO")] RESO = 1,

    [Display(Name = "MPOS")] MPOS = 2
}