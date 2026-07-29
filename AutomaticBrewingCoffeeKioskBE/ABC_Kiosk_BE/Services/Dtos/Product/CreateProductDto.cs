using Domain.Enums;
using Services.Validations;
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.Product;

public class CreateProductDto
{
    [StringLength(50)] public string? ParentId { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    public bool IsActive { get; set; }
    public string ImageUrl { get; set; }
    public ProductSize Size { get; set; }
    public ProductType Type { get; set; }

    [GreaterThan(0)] public decimal Price { get; set; }
}