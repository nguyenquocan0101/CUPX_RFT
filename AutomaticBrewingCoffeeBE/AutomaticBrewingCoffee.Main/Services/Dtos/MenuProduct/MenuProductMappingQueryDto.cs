using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.MenuProduct;

public class MenuProductMappingQueryDto : BaseQuery
{
    [MatchEnum(typeof(EBaseStatus))] public string? Status { get; set; } = null!;
}