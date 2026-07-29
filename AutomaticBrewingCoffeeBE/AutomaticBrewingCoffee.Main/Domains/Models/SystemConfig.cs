using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class SystemConfig : BaseModel
{
    [Key] [StringLength(50)] [Required] public string SystemConfigId { get; set; } = null!;
    [Required] public string Value { get; set; } = null!;
    public string? Description { get; set; } = null!;
}