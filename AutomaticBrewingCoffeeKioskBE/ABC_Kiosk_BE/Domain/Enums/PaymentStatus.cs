using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum PaymentStatus
    {
        [Display(Name = "Pending")] Pending = 0,

        /// <summary>
        /// Successful: Payment has been confirmed by the gateway via IPN/Callback, and funds are considered received/secured.
        /// </summary>
        [Display(Name = "Success")] Success = 2,

        /// <summary>
        /// Failed: The payment attempt was explicitly rejected by the gateway or financial institution (e.g., insufficient funds, declined by bank, invalid credentials).
        /// </summary>
        [Display(Name = "Failed")] Failed = 3,

        /// <summary>
        /// Cancelled: The user actively cancelled the payment process on the gateway's interface (e.g., closed the payment window/app).
        /// </summary>
        [Display(Name = "Cancelled")] Cancelled = 4,

        /// <summary>
        /// Expired: The payment request (e.g., QR code, payment link) timed out before the user completed the transaction.
        /// </summary>
        [Display(Name = "Expired")] Expired = 5,

        /// <summary>
        /// System Error: An unexpected technical error occurred during communication with the gateway or processing the request/response (e.g., network issue, invalid signature received, APIs error). Distinct from a payment failure/decline.
        /// </summary>
        [Display(Name = "Error")] Error = 6,

        // --- Refund Related Statuses ---

        /// <summary>
        /// Refunding: A refund request has been initiated for a previously successful payment and is currently being processed by the gateway. Waiting for final confirmation.
        /// </summary>
        [Display(Name = "Refunding")] Refunding = 7,

        /// <summary>
        /// Refunded: The gateway has confirmed that the refund was successfully processed and funds have been returned (or are scheduled to be returned).
        /// </summary>
        [Display(Name = "Refunded")] Refunded = 8,

        /// <summary>
        /// Refund Failed: The gateway reported that the refund attempt failed (e.g., technical issue, policy restriction).
        /// </summary>
        [Display(Name = "Refund Failed")] RefundFailed = 9
    }
}
