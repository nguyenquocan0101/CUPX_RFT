using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Menu;

public class CreateMenuDto
{
    [StringLength(100)][Required] public string Name { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }
    public BaseStatus Status { get; set; }
}