using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Models
{
    [PrimaryKey(nameof(MenuId), nameof(ProductId))]
    public partial class MenuProductMapping
    {

        [StringLength(50)][Required] public string MenuId { get; set; } = null!;

        [StringLength(50)][Required] public string ProductId { get; set; } = null!;

        public int? DisplayOrder { get; set; } = 0;

        [StringLength(10)][Required] public BaseStatus Status { get; set; } 

        [ForeignKey(nameof(MenuId))] public virtual Menu Menu { get; set; } = null!;

        [ForeignKey(nameof(ProductId))] public virtual Product Product { get; set; } = null!;
    }
}
