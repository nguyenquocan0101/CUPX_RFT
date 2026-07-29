using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public partial class Device
{
    [Key]
    [StringLength(50)]
    public string DeviceId { get; set; } = null!;
    [StringLength(50)] public string? DeviceModelId { get; set; }

    [Required]
    [StringLength(50)]
    public string SerialNumber { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(300)]
    public string Description { get; set; } = null!;

    [Required]
    [StringLength(10)]
    public DeviceStatus Status { get; set; }

    [Column(TypeName = "decimal(7, 3)")]
    public decimal X { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal Y { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal Z { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal RX { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal RY { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal RZ { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal J1 { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal J2 { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal J3 { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal J4 { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal J5 { get; set; }
    [Column(TypeName = "decimal(7, 3)")]
    public decimal J6 { get; set; }
    public virtual ICollection<DeviceLog> DeviceLogs { get; set; } = new List<DeviceLog>();
}

public class DeviceConfig : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder
            .Property(o => o.Status)
            .HasConversion(
                    v => v.ToString(),
                    v => (DeviceStatus)Enum.Parse(typeof(DeviceStatus), v)
                );
    }
}