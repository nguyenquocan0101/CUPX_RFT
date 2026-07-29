using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.KioskType;

public class KioskTypeQueryDto : BaseQuery
{
    [MatchEnum(typeof(EBaseStatus))] public string? Status { get; set; }
}
