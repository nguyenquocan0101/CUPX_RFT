using System.Text.Json.Serialization;

namespace Services.MPOS.Data;

public enum QRStatus
{
    Pending = 90,
    Rejected = 91,
    Approved = 100,
    Settled = 104,
}

public class QRStatusResponseData
{
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }
    [JsonPropertyName("posId")]
    public string? PosId { get; set; }
    [JsonPropertyName("merchantId")]
    public long MerchantId { get; set; }
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    [JsonPropertyName("transStatus")]
    public int TransStatus { get; set; }
    [JsonPropertyName("transCode")]
    public string? TransCode { get; set; }
    [JsonPropertyName("issuerCode")]
    public string? IssuerCode { get; set; }
    [JsonPropertyName("muid")]
    public string Muid { get; set; }
    [JsonPropertyName("udid")]
    public string Udid { get; set; }
    [JsonPropertyName("pan")]
    public string? Pan { get; set; }
    [JsonPropertyName("rrn")]
    public string? Rrn { get; set; }
    [JsonPropertyName("authCode")]
    public string? AuthCode { get; set; }
    [JsonPropertyName("paymentIdentifier")]
    public string? PaymentIdentifier { get; set; }
    [JsonPropertyName("qrType")]
    public string QrType { get; set; }
    [JsonPropertyName("transDate")]
    public long TransDate { get; set; }
    [JsonPropertyName("qrId")]
    public string? QrId { get; set; }
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }
    [JsonPropertyName("paymentType")]
    public string? PaymentType { get; set; }
    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }


    // Parse enum safely
    public QRStatus? TranStatusEnum => Enum.IsDefined(typeof(QRStatus), TransStatus)
        ? (QRStatus?)TransStatus
        : null;

    // Optional metadata
    public int? ResCode { get; set; }
    public string? Message { get; set; }
}

