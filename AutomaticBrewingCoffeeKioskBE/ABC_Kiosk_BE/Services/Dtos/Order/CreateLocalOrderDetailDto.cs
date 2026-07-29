using Services.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Dtos.Order
{
    public class CreateLocalOrderDetailDto
    {
        [StringLength(100)] public string ProductId { get; set; } = string.Empty;
        [Required][GreaterThan(0)] public int Quantity { get; set; }
    }
}
