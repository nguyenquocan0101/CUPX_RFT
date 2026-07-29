namespace Services.VNPay;

public class VNPayOptions
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string OrderType { get; set; } = "other";
}