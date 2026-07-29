using System.Text.Json.Serialization;

namespace Services.MPOS.Data;
/// <summary>
/// Incase of the request data for cancelling a payment which is including PosId -> it allow POS exporting invoice
/// </summary>
public class CancelQRRequestData
{
    public CancelQRRequestData(string serviceName, string orderId, string muid, string amount)
    {
        ServiceName = serviceName;
        OrderId = orderId;
        Muid = muid;
        Amount = amount;
    }
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("muid")]
    public string Muid { get; set; }
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}

