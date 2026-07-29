
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Services.Dtos.Order
{
    public class CallbackOrderDto
    {
        [Required]
        [MaxLength(50)]
        public string OrderId { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
    }
}
