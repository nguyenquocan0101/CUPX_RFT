using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class KioskType : BaseModel
{
    [Key] [StringLength(50)] [Required] public string KioskTypeId { get; set; } = null!;

    [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(200)] public string Description { get; set; } = null!;

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}