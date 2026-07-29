
using System.Text.Json.Serialization;

namespace Services.MPOS.Data
{
    public class CreateQRRequestData
    {
        public CreateQRRequestData(string serviceName, string orderId, string muid, string amount, string qrType, string? description = null)
        {
            ServiceName = serviceName;
            OrderId = orderId;
            Muid = muid;
            Amount = amount;
            QrType = qrType;
            Description = description;
        }
        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("muid")]
        public string Muid { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("qrType")]
        public string QrType { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

}
