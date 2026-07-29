namespace Services.CapRabbitMQ.Messages.Payment;

public class PaymentVNPAYCallbackMessage
{
    public string TmnCode { get; set; }
    
    public long Amount { get; set; }
    
    public string? BankCode { get; set; }
    
    public string? BankTranNo { get; set; }
    
    public string? CardType { get; set; }
    
    public string? PayDate { get; set; }
    
    public string OrderInfo { get; set; }
    
    public string TransactionNo { get; set; }
    
    public string ResponseCode { get; set; }
    
    public string TransactionStatus { get; set; }
    
    public string TxnRef { get; set; }
    
    public string SecureHash { get; set; }
    
    public string? TransactionStatusEnum { get; set; }
    
    public DateTime? PayDateParsed { get; set; }
}