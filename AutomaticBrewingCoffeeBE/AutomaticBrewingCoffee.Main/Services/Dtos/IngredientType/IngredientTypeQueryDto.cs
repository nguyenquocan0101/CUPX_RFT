using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.IngredientType;

public class IngredientTypeQueryDto : BaseQuery
{
    [MatchEnum(typeof(EBaseStatus))] public string? Status { get; set; }
}