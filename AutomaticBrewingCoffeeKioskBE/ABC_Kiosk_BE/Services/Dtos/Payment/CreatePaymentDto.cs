using Domain.Enums;
using Services.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Payment
{
    public class CreatePaymentDto
    {

        [Required]
        public string OrderId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal RequiredAmount { get; set; }
        public PaymentGateway PaymentGateway { get; set; }
    }
}
