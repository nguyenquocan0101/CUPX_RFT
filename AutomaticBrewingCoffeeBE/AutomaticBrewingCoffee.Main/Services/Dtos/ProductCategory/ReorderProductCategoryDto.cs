using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.ProductCategory;

public class ReorderProductCategoryDto
{
    [Required] public string DragProductCategoryId { get; set; } = null!;
    [Required] public string TargetProductCategoryId { get; set; } = null!;
    [Required] public bool InsertAfter { get; set; }
}