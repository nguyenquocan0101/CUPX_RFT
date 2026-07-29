using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Services.VNPay.Base;

public class VNPAYCallbackRequest
{
    [FromQuery(Name = "vnp_TmnCode")] public string TmnCode { get; set; }
    [FromQuery(Name = "vnp_Amount")] public long Amount { get; set; }
    [FromQuery(Name = "vnp_BankCode")] public string? BankCode { get; set; }
    [FromQuery(Name = "vnp_BankTranNo")] public string? BankTranNo { get; set; }
    [FromQuery(Name = "vnp_CardType")] public string? CardType { get; set; }
    [FromQuery(Name = "vnp_PayDate")] public string? PayDate { get; set; }
    [FromQuery(Name = "vnp_OrderInfo")] public string OrderInfo { get; set; }

    [FromQuery(Name = "vnp_TransactionNo")]
    public string TransactionNo { get; set; }

    [FromQuery(Name = "vnp_ResponseCode")] public string ResponseCode { get; set; }

    [FromQuery(Name = "vnp_TransactionStatus")]
    public string TransactionStatus { get; set; }

    [FromQuery(Name = "vnp_TxnRef")] public string TxnRef { get; set; }
    [FromQuery(Name = "vnp_SecureHash")] public string SecureHash { get; set; }

    public VNPayTransStatus? TransactionStatusEnum => TransactionStatus == "00"
        ? VNPayTransStatus.Success
        : VNPayTransStatus.Failed;

    public DateTime? PayDateParsed =>
        DateTime.TryParseExact(PayDate, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dt)
            ? dt
            : null;
}