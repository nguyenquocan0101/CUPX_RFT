using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.AspNetCore.Http;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

namespace Services.VNPay;

public class VNPayClient
{
    private readonly IVnpay _vnPay;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VNPayClient(IVnpay vnPay, IHttpContextAccessor httpContextAccessor)
    {
        _vnPay = vnPay;
        _httpContextAccessor = httpContextAccessor;
    }

    public string CreatePaymentUrl(Payment payment)
    {
        var paymentRequest = new PaymentRequest()
        {
            Currency = Currency.VND,
            Description = $"{payment.OrderId}",
            Language = DisplayLanguage.Vietnamese,
            Money = Double.Parse(payment.RequiredAmount.ToString()!),
            BankCode = BankCode.ANY,
            CreatedDate = DateTime.UtcNow,
            IpAddress = _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress!.ToString(),
            PaymentId = DateTime.UtcNow.Ticks,
        };
        var result = _vnPay.GetPaymentUrl(paymentRequest);
        return result;
    }
}