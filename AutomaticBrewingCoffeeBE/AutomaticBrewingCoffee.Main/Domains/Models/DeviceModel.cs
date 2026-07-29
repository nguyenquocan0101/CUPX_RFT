using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class DeviceModel : BaseModel
{
    [Key] [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;

    [StringLength(300)] public string? ModelName { get; set; } = null!;

    [StringLength(300)] public string? Manufacturer { get; set; } = null!;

    [StringLength(50)] public string? DeviceTypeId { get; set; }

    public virtual IEnumerable<DeviceFunction>? DeviceFunctions { get; set; }

    [ForeignKey(nameof(DeviceTypeId))] public virtual DeviceType? DeviceType { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    public IEnumerable<DeviceIngredient>? DeviceIngredients { get; set; } = new List<DeviceIngredient>();
}