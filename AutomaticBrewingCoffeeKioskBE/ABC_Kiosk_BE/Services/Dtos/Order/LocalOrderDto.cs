using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Services.ExternalClients;

namespace Services.Dtos.Order
{
    public class LocalOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        //public CloudOrderPaymentResponse OrderData { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsSynced { get; set; } = false;

        public ICollection<LocalOrderDetailDto> OrderDetails { get; set; }
    }
}
