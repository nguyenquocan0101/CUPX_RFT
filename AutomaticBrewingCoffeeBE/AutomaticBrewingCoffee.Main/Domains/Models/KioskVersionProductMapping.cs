using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Models;

[PrimaryKey(nameof(KioskVersionId), nameof(ProductId))]
public class KioskVersionProductMapping : BaseModel
{
    [StringLength(50)] [Required] public string KioskVersionId { get; set; } = null!;

    [ForeignKey(nameof(KioskVersionId))] public virtual KioskVersion KioskVersion { get; set; } = null!;

    [StringLength(50)] [Required] public string ProductId { get; set; } = null!;

    [ForeignKey(nameof(ProductId))] public virtual Product Product { get; set; } = null!;
}