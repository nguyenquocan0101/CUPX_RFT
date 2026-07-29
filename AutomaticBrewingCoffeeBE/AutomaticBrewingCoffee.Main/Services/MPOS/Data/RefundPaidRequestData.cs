
using System.Text.Json.Serialization;

namespace Services.MPOS.Data
{
    public class RefundPaidRequestData
    {
        public RefundPaidRequestData(string serviceName,  string orderId, long amount,string? transCode = null, string? posId = null)
        {
            ServiceName = serviceName;
            TransCode = transCode;
            OrderId = orderId;
            PosId = posId;
            RequestId = Guid.NewGuid().ToString(); //will be saved in referenceId in Payment
            RefundAmount = amount;
        }
        [JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [JsonPropertyName("transCode")]
        public string? TransCode { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("posId")]
        public string? PosId { get; set; }
        [JsonPropertyName("requestId")] //unique id for refund request, auto generated when no added 
        public string? RequestId { get; set; }

        [JsonPropertyName("refundAmount")]
        public long RefundAmount { get; set; }
    }
}
