using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;

namespace Services.AzureIotHub
{
    public class DeviceManager(IConfiguration configuration)
    {
        private readonly string iotHubOwner = configuration["AzureIotHub:HubOwner"]!;
        private readonly RegistryManager _registryManager = RegistryManager.CreateFromConnectionString(configuration["AzureIotHub:HubOwner"]);

        public async Task<Device?> AddHubDevice(string deviceId)
        {
            var newDevice = new Device(deviceId);
            Device? device = await _registryManager.AddDeviceAsync(newDevice);
            return device;
        }

        public async Task<Device?> GetHubDevice(string deviceId)
        {
            Device? device = await _registryManager.GetDeviceAsync(deviceId);
            return device;
        }

        public async Task<bool> RemoveHubDevice(string deviceId)
        {
            try
            {
                await _registryManager.RemoveDeviceAsync(deviceId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string BuildDevicePrimaryConnectionStr(Device device)
        {
            var dConStr = $"HostName={GetIotHubHostName()};DeviceId={device.Id};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}";
            return dConStr;
        }

        string GetIotHubHostName()
        {
            // Lấy HostName từ connection string
            var parts = iotHubOwner.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("HostName=", StringComparison.InvariantCultureIgnoreCase))
                {
                    return part.Substring("HostName=".Length);
                }
            }
            throw new Exception("Không tìm thấy HostName trong connection string.");
        }
    }
}