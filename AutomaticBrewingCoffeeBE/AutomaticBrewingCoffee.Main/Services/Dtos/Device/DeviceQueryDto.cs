using AutomaticBrewingCoffee.Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Device
{
    public class DeviceQueryDto : BaseQuery
    {
        [MatchEnum(typeof(EDeviceStatus))] public string? Status { get; set; }
        
        public string? DeviceModelId { get; set; }
        
    }
}