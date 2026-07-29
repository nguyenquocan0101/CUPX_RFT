using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Payment
{
    public class PaymentDto
    {
        public string PaymentId { get; set; } = null!;

        public string? OrderId { get; set; }

        public string? PaymentContent { get; set; }

        public decimal? RequiredAmount { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public string? PaymentDestinationId { get; set; }

        public decimal? PaidAmount { get; set; }

        public decimal? RefundedAmount { get; set; }

        public string? PaymentStatus { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}