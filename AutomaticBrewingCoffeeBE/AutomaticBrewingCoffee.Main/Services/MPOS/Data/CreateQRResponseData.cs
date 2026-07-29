
using System.Text.Json.Serialization;

namespace Services.MPOS.Data
{

    public class CreateQRResponseData
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("merchantId")]
        public long MerchantId { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("muid")]
        public string Muid { get; set; }

        [JsonPropertyName("udid")]
        public string Udid { get; set; }

        [JsonPropertyName("qrType")]
        public string QrType { get; set; }

        [JsonPropertyName("qrId")]
        public string QrId { get; set; }

        [JsonPropertyName("qrCode")]
        public string QrCode { get; set; }
    }
}
