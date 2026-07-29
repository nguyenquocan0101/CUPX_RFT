using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    /// <summary>
    /// Specifies the fulfillment timing type for an order.
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// The order is placed for immediate preparation and fulfillment.
        /// </summary>
        [Display(Name = "Immediate")]
        Immediate = 1,

        /// <summary>
        /// The order is placed in advance for fulfillment at a later time (e.g., scheduled pickup).
        /// </summary>
        [Display(Name = "Pre-Order")]
        PreOrder = 2 
    }
}
