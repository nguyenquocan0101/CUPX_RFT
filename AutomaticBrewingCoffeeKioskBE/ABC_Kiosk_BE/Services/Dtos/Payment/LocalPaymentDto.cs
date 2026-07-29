using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Payment
{
    public class LocalPaymentDto
    {
        public string? PaymentId { get; set; } = null;
        public string? PaymentUrl { get; set; } = null;
        public string? PaymentQr { get; set; } = null;
        public decimal RequiredAmount { get; set; }
    }
}
