using Domain.Enums;

namespace Services.Dtos.Order
{
    public class OrderQueryDto 
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public bool IsAsc { get; set; } = true;
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
