using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Kiosk

{
    public class KioskQueryDto : BaseQuery
    {
        [MatchEnum(typeof(EBaseStatus))] public string? Status { get; set; }
        public string? StoreId { get; set; }

        public string? OrganizationId { get; set; }
    }
}