using System.ComponentModel.DataAnnotations;
using Services.Validations;

namespace Services.Dtos.ProductCategory;

public class UpdateProductCategoryDto
{
    [Required] [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    [MatchBase64] public string? ImageBase64 { get; set; }

    public string? ImageUrl { get; set; }

    public int? DisplayOrder { get; set; } = 0;
}