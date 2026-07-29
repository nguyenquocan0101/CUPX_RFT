using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.IngredientType;

public class UpdateIngredientTypeDto
{
    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}