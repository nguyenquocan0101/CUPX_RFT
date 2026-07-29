using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class LocationType : BaseModel
{
    [Key] [StringLength(50)] [Required] public string LocationTypeId { get; set; } = null!;

    [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(200)] public string? Description { get; set; }
}