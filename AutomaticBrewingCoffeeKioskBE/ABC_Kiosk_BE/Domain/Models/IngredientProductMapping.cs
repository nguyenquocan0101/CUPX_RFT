using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public partial class IngredientProductMapping
{
    [StringLength(50)]
    public string IngredientId { get; set; } = null!;

    [StringLength(50)]
    public string ProductId { get; set; } = null!;
    public double? Quantity { get; set; }

    [ForeignKey("IngredientId")]
    public virtual Ingredient Ingredient { get; set; } = null!;

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}

public class IngredientProductMappingConfig : IEntityTypeConfiguration<IngredientProductMapping>
{
    public void Configure(EntityTypeBuilder<IngredientProductMapping> builder)
    {
        builder.HasKey(ipm => new { ipm.IngredientId, ipm.ProductId });
    }
}