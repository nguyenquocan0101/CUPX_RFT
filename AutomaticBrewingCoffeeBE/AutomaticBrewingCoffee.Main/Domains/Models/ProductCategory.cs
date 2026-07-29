using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Models;

public class ProductCategory : BaseModel
{
    [StringLength(50)] [Required] public string ProductCategoryId { get; set; } = null!;

    [Required] [StringLength(100)] public string Name { get; set; } = null!;

    [StringLength(450)] public string? Description { get; set; }

    [StringLength(10)] [Required] public string Status { get; set; } = null!;
    
    public string? ImageUrl { get; set; }
    
    public virtual IEnumerable<Product>? Products { get; set; }
    
    public int? DisplayOrder { get; set; } = 0;
    
}