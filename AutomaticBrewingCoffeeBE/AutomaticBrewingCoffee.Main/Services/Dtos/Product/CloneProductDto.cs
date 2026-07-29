using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Product;

public class CloneProductDto
{
    [StringLength(50)] [Required] public string ProductId { get; set; } = null!;
}