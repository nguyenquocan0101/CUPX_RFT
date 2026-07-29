
using System.Text.Json.Serialization;

namespace Services.MPOS.Data
{
    public class RefundPaidResponseData
    {
        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [JsonPropertyName("transCode")]
        public string? TransCode { get; set; }

        [JsonPropertyName("requestId")] //unique id for refund request, auto generated when no added 
        public string? RequestId { get; set; }

        [JsonPropertyName("refundAmount")]
        public long RefundAmount { get; set; }
        [JsonPropertyName("restedAmount")]
        public long RestedAmount { get; set; }

        // Optional metadata
        public int? ResCode { get; set; }
        public string? Message { get; set; }
    }
}
