namespace Services.Dtos.Order;

public class OrderInsideDto
{
    public string OrderId { get; set; } = null!;

    public string? OrderCode { get; set; }

    public string KioskId { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public string? OrganizationId { get; set; } = null!;

    public string? StoreId { get; set; } = null!;

    public decimal? Discount { get; set; }

    public string? DiscountCode { get; set; }

    public decimal? FinalAmount { get; set; }

    public string? OrderType { get; set; }

    public string? PaymentGateway { get; set; }

    public string? Status { get; set; }

    public string? LastUpdateBy { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateTime? PendingDate { get; set; }

    public DateTime? PreparingDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public DateTime? FailedDate { get; set; }
}