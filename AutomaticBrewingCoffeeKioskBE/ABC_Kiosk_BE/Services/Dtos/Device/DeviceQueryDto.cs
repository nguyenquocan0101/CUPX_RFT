using Domain.Enums;
using Services.Base;
using Services.Validations;

namespace Services.Dtos.Device
{
    public class DeviceQueryDto : BaseQuery
    {
        public DeviceStatus? Status { get; set; }
    }
}
