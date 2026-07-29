using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomaticBrewingCoffee.Domain.Models;

public class Menu : BaseModel
{
    [Key] [StringLength(50)] [Required] public string MenuId { get; set; } = null!;

    [StringLength(50)] public string? OrganizationId { get; set; }


    [ForeignKey(nameof(OrganizationId))]
    [StringLength(50)]
    public virtual Organization? Organization { get; set; }

    [StringLength(100)] [Required] public string Name { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;

    public virtual ICollection<MenuProductMapping>? MenuProductMappings { get; set; } = new List<MenuProductMapping>();
}