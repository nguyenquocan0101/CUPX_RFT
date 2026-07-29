using Services.Dtos.Kiosk;
using Services.Dtos.OrderDetail;
using Services.Dtos.Organization;
using Services.Dtos.Payment;
using Services.Dtos.Product;
using Services.Dtos.Store;

namespace Services.Dtos.Order
{
    public class OrderDto
    {
        public string OrderId { get; set; } = null!;

        public string? OrderCode { get; set; }

        public string KioskId { get; set; } = null!;

        public string ClientId { get; set; } = null!;

        public string? OrganizationId { get; set; } = null!;
        
        public string? StoreId { get; set; } = null!;

        public KioskInOrderDto? Kiosk { get; set; }

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

        public virtual ICollection<OrderDetailDto> OrderDetails { get; set; } =
            new List<OrderDetailDto>();

        public virtual ICollection<PaymentDto> Payments { get; set; } =
            new List<PaymentDto>();

        public List<ProductExecuteDto>? CompletedProducts { get; set; }

        public List<ProductExecuteDto>? PreparingProducts { get; set; }

        public List<ProductExecuteDto>? FailedProducts { get; set; }

        // Payment checkout resource

        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? PaymentQr { get; set; }

        public DateTime? ExpiredDate { get; set; }
    }
}