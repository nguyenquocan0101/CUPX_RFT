using Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Models;

public partial class LocalOrderDetail
{
    [Key]
    [StringLength(50)]
    public string OrderDetailId { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string? OrderId { get; set; }
    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = null!;
    [StringLength(10)]
    public ProductSize? Size { get; set; }
    public int Quantity { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TotalAmount { get; set; }
    [Required]
    public string DetailData { get; set; } = null!;
    public bool? IsSynced { get; set; } = false; 

    [ForeignKey("OrderId")]
    [JsonIgnore]
    public virtual LocalOrder? Order { get; set; }
}

public class LocalOrderDetailConfig : IEntityTypeConfiguration<LocalOrderDetail>
{
    public void Configure(EntityTypeBuilder<LocalOrderDetail> builder)
    {
        builder
         .Property(o => o.Size)
         .HasConversion(
                 v => v.ToString(),
                 v => (ProductSize)Enum.Parse(typeof(ProductSize), v)
             );
    }
}