using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Kiosk
{
    public class CreateKioskDto
    {
        // DB: varchar(100), Not Null
        [Required(ErrorMessage = "DeviceIds is required.")]
        public List<String> DeviceIds { get; set; } = new();
        
        //[Required(ErrorMessage = "FranchiseId is required.")]
        public string? FranchiseId { get; set; } = null!;

        // DB: varchar(100), Nullable
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string? Location { get; set; }
        public BaseStatus Status { get; set; } = default!;

        // DB: datetime, Not Null
        [Required(ErrorMessage = "InstalledDate is required.")]
        public DateTime InstalledDate { get; set; }
    }
}
