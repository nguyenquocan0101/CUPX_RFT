using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class FunctionParameter : BaseModel
{
    [Key] [StringLength(50)] [Required] public string FunctionParameterId { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceFunctionId { get; set; } = null!;

    [ForeignKey(nameof(DeviceFunctionId))] public virtual DeviceFunction DeviceFunction { get; set; } = null!;

    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(50)] public string Type { get; set; } = null!;

    [StringLength(450)] public string? Min { get; set; } = null!;

    [StringLength(2048)] public string Options { get; set; } = string.Empty;

    [StringLength(450)] public string? Max { get; set; } = null!;

    [StringLength(450)] public string Default { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; } = null!;
}