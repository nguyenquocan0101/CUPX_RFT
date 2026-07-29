using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    /// <summary>
    /// Represents the operational status of a specific device connected to a Kiosk.
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// The device is added in stock
        /// </summary>
        [Display(Name = "Stock")] Stock = 0,

        /// <summary>
        /// The device is connected in Kiosk
        /// </summary>
        [Display(Name = "Working")] Working = 1,

        /// <summary>
        /// The device is connect/disconected but it is being maintained
        /// </summary>
        [Display(Name = "Maintain")] Maintain = 2,

    }
}