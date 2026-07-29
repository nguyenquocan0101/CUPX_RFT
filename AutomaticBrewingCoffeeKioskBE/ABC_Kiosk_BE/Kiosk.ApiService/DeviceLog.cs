using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;


public class DeviceLog
{
    [Key] // Primary Key
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Corresponds to IDENTITY(1, 1)
    public int LogId { get; set; }

    [StringLength(50)] 
    public string DeviceId { get; set; } 

    [StringLength(100)] 
    public string LogKey { get; set; }

    public string LogValue { get; set; }

    [StringLength(10)] 
    public DeviceLogType LogType { get; set; }

    public string Response { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; }
}

public class DeviceLogConfig : IEntityTypeConfiguration<DeviceLog>
{
    public void Configure(EntityTypeBuilder<DeviceLog> builder)
    {
        builder
         .Property(o => o.LogType)
         .HasConversion(
                 v => v.ToString(),
                 v => (DeviceLogType)Enum.Parse(typeof(DeviceLogType), v)
             );
    }
}   