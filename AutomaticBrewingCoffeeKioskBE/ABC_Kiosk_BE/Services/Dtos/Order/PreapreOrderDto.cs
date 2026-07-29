using Domain.Enums;
using Services.ExternalClients;

namespace Services.Dtos.Order
{
    public class PrepareOrderDto
    {
        public string OrderId { get; set; } = null!;

        public decimal? Discount { get; set; }

        public decimal? FinalAmount { get; set; }

        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        // public string? OrderType { get; set; }
        public string? PaymentUrl { get; set; } = null;
        public string? PaymentQr { get; set; } = null;
        public virtual ICollection<LocalOrderDetailDto> OrderDetails { get; set; } = new List<LocalOrderDetailDto>();
    }
}
