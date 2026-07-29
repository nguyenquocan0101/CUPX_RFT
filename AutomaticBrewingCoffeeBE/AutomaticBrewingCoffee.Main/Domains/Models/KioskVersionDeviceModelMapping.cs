using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

[PrimaryKey(nameof(KioskVersionId), nameof(DeviceModelId))]
public class KioskVersionDeviceModelMapping : BaseModel
{
    [StringLength(50)] [Required] public string KioskVersionId { get; set; } = null!;

    [ForeignKey(nameof(KioskVersionId))] public virtual KioskVersion KioskVersion { get; set; } = null!;

    [StringLength(50)] [Required] public string DeviceModelId { get; set; } = null!;

    [ForeignKey(nameof(DeviceModelId))] public virtual DeviceModel DeviceModel { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}