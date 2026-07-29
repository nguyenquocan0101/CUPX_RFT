using System.Text.Json.Serialization;

namespace Services.MPOS.Base;

public partial class Response
{
    [JsonPropertyName("resData")] public string ResData { get; set; }
    [JsonPropertyName("resCode")] public int ResCode { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; }
}

//remove_QR need this property
public partial class Response
{
    [JsonPropertyName("merchantID")] public int? MerchantID { get; set; }
}

public class MPOSCallbackResponse
{
    [JsonPropertyName("resCode")] public int ResCode { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; }
}