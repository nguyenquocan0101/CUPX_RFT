using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    /// <summary>
    /// Represents the operational status of a specific device connected to a Kiosk.
    /// </summary>
    public enum KioskDeviceStatus
    {
        /// <summary>
        /// The device in Kiosk is connected
        /// </summary>
        [Display(Name = "Online")]
        Online = 0,

        /// <summary>
        /// The device in Kiosk is not connected
        /// </summary>
        [Display(Name = "Offline")]
        Offline = 1,

        /// <summary>
        /// The device is reporting a non-critical issue 
        /// </summary>
        [Display(Name = "Warning")]
        Warning = 2,

        /// <summary>
        /// The device has reported a critical failure and cannot perform its intended function.
        /// </summary>
        [Display(Name = "Error")]
        Error = 3,

        /// <summary>
        /// The device is not recognized/configured in this Kiosk 
        /// </summary>
        [Display(Name = "Disabled")]
        Disabled = 4,
    }
}