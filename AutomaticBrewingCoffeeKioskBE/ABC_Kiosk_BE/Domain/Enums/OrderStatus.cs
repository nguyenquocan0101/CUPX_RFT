using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum OrderStatus
    {
        [Display(Name = "Pending")] Pending = 0,

        /// <summary>
        /// The order items are actively being prepared by the kiosk.
        /// </summary>
        [Display(Name = "Preparing")] Preparing = 1,

        /// <summary>
        /// The order has been successfully fulfilled
        /// </summary>
        [Display(Name = "Completed")] Completed = 2,

        /// <summary>
        /// The order was cancelled before Preparing
        /// </summary>
        [Display(Name = "Cancelled")] Cancelled = 3,

        /// <summary>
        /// The order could not be completed due to an unrecoverable error after confirmation (e.g., machine malfunction during preparation).
        /// </summary>
        [Display(Name = "Failed")] Failed = 4
    }
}
