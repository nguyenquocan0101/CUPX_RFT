using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Services.Validations;

namespace Services.Dtos.Product;

public class UpdateProductDto
{
    [StringLength(50)] public string? ParentId { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
    public ProductSize Size { get; set; }

    public ProductType Type { get; set; }

    [GreaterThan(0)] public decimal Price { get; set; }
}