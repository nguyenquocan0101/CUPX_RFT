using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public partial class Menu
{
    [Key][StringLength(50)][Required] public string MenuId { get; set; } = null!;

    [StringLength(100)][Required] public string Name { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(10)][Required] public BaseStatus Status { get; set; } 

    public virtual ICollection<MenuProductMapping> MenuProductMappings { get; set; } = new List<MenuProductMapping>();
}

public class MenuConfig : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder
            .Property(o => o.Status)
            .HasConversion(
                    v => v.ToString(),
                    v => (BaseStatus)Enum.Parse(typeof(BaseStatus), v)
                );
    }
}