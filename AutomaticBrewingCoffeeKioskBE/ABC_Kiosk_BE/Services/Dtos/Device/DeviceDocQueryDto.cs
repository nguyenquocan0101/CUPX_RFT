
using CouchDb.Domain.Enums;

namespace Services.Dtos.Device
{
    public class DeviceDocQueryDto
    {
        public string? DeviceModelId { get; set; }
        public EWorkingStatus? WorkingStatus { get; set; }
    }
}
