using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class IngredientType : BaseModel
{
    [Key] [StringLength(50)] [Required] public string IngredientTypeId { get; set; } = null!;

    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}