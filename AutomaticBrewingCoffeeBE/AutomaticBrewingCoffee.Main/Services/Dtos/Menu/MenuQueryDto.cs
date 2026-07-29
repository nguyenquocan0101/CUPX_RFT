using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Menu;

public class MenuQueryDto : BaseQuery
{
    [MatchEnum(typeof(EBaseStatus))] public string? Status { get; set; }
    public string? OrganizationId { get; set; }
}