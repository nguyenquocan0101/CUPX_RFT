using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutomaticBrewingCoffee.Domain.Enums;

namespace AutomaticBrewingCoffee.Domain.Models;

public class KioskDeviceMapping : BaseModel
{
    [Key] [StringLength(50)] [Required] public string KioskDeviceMappingId { get; set; } = null!;

    [StringLength(50)] public string? DeviceId { get; set; }

    [StringLength(50)] public string? KioskId { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;


    [StringLength(10)] public string? Side { get; set; }

    public bool IsDisposed { get; set; } = false;

    public DateTime? DisposedDate { get; set; }

    [StringLength(450)] public string? Note { get; set; }

    [ForeignKey(nameof(DeviceId))] public virtual Device? Device { get; set; }

    [ForeignKey(nameof(KioskId))] public virtual Kiosk? Kiosk { get; set; }

    public void Online(string note)
    {
        Status = EKioskDeviceStatus.Online.ToString();
        UpdatedDate = DateTime.UtcNow;
        Note = note;
    }

    public void Offline(string note)
    {
        Status = EKioskDeviceStatus.Offline.ToString();
        UpdatedDate = DateTime.UtcNow;
        Note = note;
    }

    public void Warning(string note)
    {
        Status = EKioskDeviceStatus.Warning.ToString();
        UpdatedDate = DateTime.UtcNow;
        Note = note;
    }

    public void Error(string note)
    {
        Status = EKioskDeviceStatus.Error.ToString();
        UpdatedDate = DateTime.UtcNow;
        Note = note;
    }

    public void Dispose()
    {
        IsDisposed = true;
        DisposedDate = DateTime.UtcNow;
    }
}