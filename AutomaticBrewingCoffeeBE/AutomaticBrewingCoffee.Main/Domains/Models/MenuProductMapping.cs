using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

[PrimaryKey(nameof(MenuId), nameof(ProductId))]
public class MenuProductMapping : BaseModel
{
    [StringLength(50)] [Required] public string MenuId { get; set; } = null!;

    [StringLength(50)] [Required] public string ProductId { get; set; } = null!;

    public int? DisplayOrder { get; set; } = 0;

    [Precision(18, 2)]
    public decimal? SellingPrice { get; set; } = 0;

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    [ForeignKey(nameof(MenuId))] public virtual Menu Menu { get; set; } = null!;

    [ForeignKey(nameof(ProductId))] public virtual Product Product { get; set; } = null!;
}