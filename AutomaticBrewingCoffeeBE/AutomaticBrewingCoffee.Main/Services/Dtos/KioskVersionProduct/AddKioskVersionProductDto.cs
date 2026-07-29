using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.KioskVersionProduct;

public class AddKioskVersionProductDto
{
    [StringLength(50)] [Required] public string KioskVersionId { get; set; } = null!;

    [StringLength(50)] [Required] public string ProductId { get; set; } = null!;
    
}