using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Menu;

public class CloneMenuDto
{
    [Required] [StringLength(50)] public string MenuId { get; set; } = null!;
}