using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.VNPay.Base;

public class VNPAYCallbackResponse
{
    [JsonPropertyName("RspCode")] public string RspCode { get; set; }

    [JsonPropertyName("Message")] public string Message { get; set; }

    private VNPAYCallbackResponse(string rspCode, string message)
    {
        RspCode = rspCode;
        Message = message;
    }

    public override string ToString() => JsonSerializer.Serialize(this);

    public static VNPAYCallbackResponse Success() =>
        new("00", "Confirm Success");

    public static VNPAYCallbackResponse OrderNotFound() =>
        new("01", "Order not found");

    public static VNPAYCallbackResponse AlreadyConfirmed() =>
        new("02", "Order already confirmed");

    public static VNPAYCallbackResponse InvalidAmount() =>
        new("04", "Invalid amount");

    public static VNPAYCallbackResponse InvalidSignature() =>
        new("97", "Invalid signature");

    public static VNPAYCallbackResponse MissingData() =>
        new("99", "Input data required");
}