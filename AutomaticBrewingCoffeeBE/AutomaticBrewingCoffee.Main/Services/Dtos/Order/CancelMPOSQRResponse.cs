using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Order
{
    public class CancelMPOSQRResponse
    {
        public string OrderId { get; set; }
        public int? ResCode { get; set; }
        public string? Message { get; set; }
    }
}
