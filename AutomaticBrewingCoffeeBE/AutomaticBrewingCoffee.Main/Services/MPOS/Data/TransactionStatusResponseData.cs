using System.Text.Json.Serialization;

namespace Services.MPOS.Data;

public enum MPOSTransStatus
{
    Approved = 100,
    Reversed = 101,
    Voided = 102,
    PendingSignature = 103,
    Settled = 104,
    Pending= 90,
    Rejected = 91,
    Refunded = 99,
    Fail = 97
}

public class TransactionStatusResponseData
{
    [JsonPropertyName("serviceName")] public string ServiceName { get; set; }

    [JsonPropertyName("transStatus")] public int TransStatus { get; set; }

    [JsonPropertyName("transCode")] public string TransCode { get; set; }

    [JsonPropertyName("transDate")] public long TransDate { get; set; }

    [JsonPropertyName("transAmount")] public long TransAmount { get; set; }

    [JsonPropertyName("issuerCode")] public string IssuerCode { get; set; }

    [JsonPropertyName("muid")] public string Muid { get; set; }

    [JsonPropertyName("orderId")] public string OrderId { get; set; }

    [JsonPropertyName("posId")] public string PosId { get; set; }

    public MPOSTransStatus? TranStatusEnum => Enum.IsDefined(typeof(MPOSTransStatus), TransStatus)
        ? (MPOSTransStatus?)TransStatus
        : null;
}