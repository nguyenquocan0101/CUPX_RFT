using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Product;

public class ProductQueryDto : BaseQuery
{
    [MatchEnum(typeof(EProductStatus))] public string? Status { get; set; }
    public string? ProductSize { get; set; }
    public string? ProductType { get; set; }
    
    public string? CategoryName { get; set; }
    public string? TagName { get; set; }
    
    public bool? IsHasWorkflow { get; set; }
}