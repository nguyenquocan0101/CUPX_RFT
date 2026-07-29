using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;


namespace Domain.Models;

public partial class LocalOrder
{
    [Key]
    [StringLength(50)]
    public string OrderId { get; set; } = string.Empty;

    [Required]
    public string OrderData { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public OrderStatus Status { get; set; } 

    public DateTime? CreatedAt { get; set; }

    public bool? IsSynced { get; set; } = false;

    public ICollection<LocalOrderDetail> OrderDetails { get; set; }
}

public class LocalOrderConfig : IEntityTypeConfiguration<LocalOrder>
{
    public void Configure(EntityTypeBuilder<LocalOrder> builder)
    {
        builder
         .Property(o => o.Status)
         .HasConversion(
                 v => v.ToString(),
                 v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v)
             );
    }
}