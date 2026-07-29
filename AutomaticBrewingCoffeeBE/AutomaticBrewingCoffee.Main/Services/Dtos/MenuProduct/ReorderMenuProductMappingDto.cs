using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.MenuProduct;

public class ReorderMenuProductMappingDto
{
    [Required] public string DragProductId { get; set; } = null!;
    [Required] public string TargetProductId { get; set; } = null!;
    [Required] public bool InsertAfter { get; set; }
}