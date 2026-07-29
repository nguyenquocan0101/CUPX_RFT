using System.Text.Json.Serialization;

namespace Services.MPOS.Base;

public class Request
{
    [JsonPropertyName("merchantId")] public string MerchantId { get; set; }
    [JsonPropertyName("reqData")] public string ReqData { get; set; }
}

public class MPOSCallbackRequest
{
    [JsonPropertyName("merchantID")] public long MerchantId { get; set; }
    [JsonPropertyName("reqData")] public string ReqData { get; set; } 
}