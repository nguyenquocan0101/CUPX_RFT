using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Device : BaseModel
{
    [Key] [StringLength(50)] [Required] public string DeviceId { get; set; } = null!;

    [StringLength(50)] public string? DeviceModelId { get; set; }

    [ForeignKey(nameof(DeviceModelId))] public virtual DeviceModel? DeviceModel { get; set; }

    [StringLength(100)] public string SerialNumber { get; set; } = null!;

    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(300)] [Required] public string Description { get; set; } = null!;

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    public bool IsOnHub { get; set; }

    public IEnumerable<DeviceIngredientHistory>? DeviceIngredientHistories { get; set; }

    public IEnumerable<DeviceIngredientState>? DeviceIngredientStates { get; set; }

    public void Stock()
    {
        Status = EDeviceStatus.Stock.ToString();
        UpdatedDate = DateTime.UtcNow;
    }

    public void Working()
    {
        Status = EDeviceStatus.Working.ToString();
        UpdatedDate = DateTime.UtcNow;
    }

    public void Maintain()
    {
        Status = EDeviceStatus.Maintain.ToString();
        UpdatedDate = DateTime.UtcNow;
    }

    public void OnHub()
    {
        IsOnHub = true;
        UpdatedDate = DateTime.UtcNow;
    }

    public void DownHub()
    {
        IsOnHub = false;
        UpdatedDate = DateTime.UtcNow;
    }
}