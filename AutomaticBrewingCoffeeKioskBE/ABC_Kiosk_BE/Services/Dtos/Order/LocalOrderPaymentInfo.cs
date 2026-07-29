namespace Services.Dtos.Order;

/// <summary>
/// Class used for Data props in LocalPayment
/// </summary>
public class LocalOrderPaymentInfo
{
    public string PaymentUrl { get; set; } = string.Empty;
    public string PaymentQr { get; set; } = string.Empty;
}