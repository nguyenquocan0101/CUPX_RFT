using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class DeviceFunction : BaseModel
{
    [Key] [StringLength(50)] [Required] public string DeviceFunctionId { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;

    [ForeignKey(nameof(DeviceModelId))] public virtual DeviceModel DeviceModel { get; set; } = null!;


    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(100)] public string? Label { get; set; }

    public virtual IEnumerable<FunctionParameter>? FunctionParameters { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
}