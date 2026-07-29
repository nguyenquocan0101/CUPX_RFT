using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum DeviceLogType
    {
        [Display(Name = "Information")]
        Info = 0,      
        [Display(Name = "Error")]
        Error = 1,      
        [Display(Name = "Unknown")]
        Unknown =  2
    }
}