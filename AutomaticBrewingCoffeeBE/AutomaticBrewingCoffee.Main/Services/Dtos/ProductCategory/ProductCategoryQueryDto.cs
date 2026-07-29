using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.ProductCategory;

public class ProductCategoryQueryDto : BaseQuery
{
    [MatchEnum(typeof(EBaseStatus))] public string? Status { get; set; }
}