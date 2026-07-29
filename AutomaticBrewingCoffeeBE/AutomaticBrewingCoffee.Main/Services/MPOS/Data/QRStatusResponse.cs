namespace Services.MPOS.Data
{
    public class QRStatusResponse
    {
        public QRStatus? Status { get; set; }
        public string? OrderStatus { get; set; }
        public long Amount { get; set; }
        public string? QrType { get; set; }
        public long TransDate { get; set; }
        public int? ResCode { get; set; }
        public string? Message { get; set; }

    }
}
