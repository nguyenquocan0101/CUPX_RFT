using System.Text.Json.Serialization;

namespace Services.MPOS.Data
{
    public class QRStatusQueryData
    {
        public QRStatusQueryData(string serviceName, string orderId, string muid, string amount)
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
}
