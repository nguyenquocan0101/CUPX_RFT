using Domain.CouchDbModels;
using Services.Base;
using Services.Dtos.Device;

namespace Services.Interfaces
{
    public interface IDeviceService2
    {
        Task<BaseResult<DeviceDocument[]>> GetAllDeviceDocsAsync(DeviceDocQueryDto query);

    }
}
