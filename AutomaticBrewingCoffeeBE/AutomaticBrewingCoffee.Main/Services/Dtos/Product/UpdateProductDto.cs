using System.ComponentModel.DataAnnotations;
using AutomaticBrewingCoffee.Domain.Enums;
using Services.Dtos.ProductAttribute;
using Services.Validations;

namespace Services.Dtos.Product;

public class UpdateProductDto
{
    [StringLength(50)] public string? ParentId { get; set; }

    [StringLength(50)] public string? ProductCategoryId { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(300)] public string? Description { get; set; }

    [MatchEnum(typeof(EProductStatus))] public string Status { get; set; } = null!;

    [StringLength(100)] public string? TagName { get; set; }

    [Required]
    [MatchEnum(typeof(EProductSize))]
    public string Size { get; set; }

    [MatchBase64] public string? ImageBase64 { get; set; }
    
    public string? ImageUrl { get; set; }

    [Required]
    [MatchEnum(typeof(EProductType))]
    public string Type { get; set; }

    [GreaterThan(0)] public decimal Price { get; set; }

    public List<ProductAttributeNestedDto>? ProductAttributes { get; set; }
}