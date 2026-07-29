
using System.Text.Json.Serialization;

namespace Services.MPOS.Data
{
    public class CancelQRResponseData
    {
        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }
        [JsonPropertyName("merchantId")]
        public long MerchantId { get; set; }

        [JsonPropertyName("muid")]
        public string Muid { get; set; }
        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        //Metadata
        public int? ResCode { get; set; }
        public string? Message { get; set; }

    }
}
