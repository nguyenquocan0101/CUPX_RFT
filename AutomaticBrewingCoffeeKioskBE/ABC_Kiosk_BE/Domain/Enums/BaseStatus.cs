using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    /// <summary>
    /// Represents the basic operational status of various entities like Kiosks, Devices, Menus, etc.
    /// </summary>
    public enum BaseStatus
    {
        /// <summary>
        /// The entity is currently operational, available, or enabled.
        /// </summary>
        [Display(Name = "Active")]
        Active = 0,

        /// <summary>
        /// The entity is not operational, unavailable, disabled, or logically deleted.
        /// </summary>
        [Display(Name = "Inactive")]
        Inactive = 1
    }
}