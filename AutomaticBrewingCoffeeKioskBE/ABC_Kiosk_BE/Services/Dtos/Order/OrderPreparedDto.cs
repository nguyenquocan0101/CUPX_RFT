using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Order
{
    public class OrderPreparedDto
    {
        public OrderPreparedDto(string orderId, string paymentUrl, string paymentQr, DateTime? orderDate, ICollection<LocalOrderDetailDto> orderDetails)
        {
            OrderId = orderId;
            PaymentUrl = paymentUrl;
            PaymentQr = paymentQr;
            OrderDate = orderDate;
            OrderDetails = orderDetails;
        }
        public string OrderId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string PaymentQr { get; set; } = string.Empty;
        public DateTime? OrderDate { get; set; }
        public ICollection<LocalOrderDetailDto> OrderDetails { get; set; }
    }
}
