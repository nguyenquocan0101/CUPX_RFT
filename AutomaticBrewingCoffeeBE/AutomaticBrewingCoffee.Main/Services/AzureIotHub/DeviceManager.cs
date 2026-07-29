using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;

namespace Services.AzureIotHub
{
public class DeviceManager(IConfiguration configuration)
{
        private readonly bool _localMode = string.Equals(
            configuration["ASPNETCORE_ENVIRONMENT"],
            "Local",
            StringComparison.OrdinalIgnoreCase)
            || configuration.GetValue<bool>("LOCAL_MODE");
        private readonly string _iotHubOwner = configuration["AzureIotHub:HubOwner"] ?? string.Empty;
        private readonly RegistryManager? _registryManager = CreateRegistryManager(configuration);

        private static RegistryManager? CreateRegistryManager(IConfiguration configuration)
        {
            var localMode = string.Equals(
                configuration["ASPNETCORE_ENVIRONMENT"],
                "Local",
                StringComparison.OrdinalIgnoreCase)
                || configuration.GetValue<bool>("LOCAL_MODE");
            if (localMode) return null;

            return RegistryManager.CreateFromConnectionString(
                configuration["AzureIotHub:HubOwner"]!);
        }

        public async Task<Device?> AddHubDevice(string deviceId)
        {
            if (_localMode)
                return new Device(deviceId);

            var newDevice = new Device(deviceId);
            Device? device = await _registryManager!.AddDeviceAsync(newDevice);
            return device;
        }

        public async Task<Device?> GetHubDevice(string deviceId)
        {
            if (_localMode) return null;

            Device? device = await _registryManager!.GetDeviceAsync(deviceId);
            return device;
        }

        public async Task<bool> RemoveHubDevice(string deviceId)
        {
            if (_localMode) return true;

            try
            {
                await _registryManager!.RemoveDeviceAsync(deviceId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string BuildDevicePrimaryConnectionStr(Device device)
        {
            if (_localMode)
                return $"local://{device.Id}";

            var dConStr = $"HostName={GetIotHubHostName()};DeviceId={device.Id};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}";
            return dConStr;
        }

        private string GetIotHubHostName()
        {
            // Lấy HostName từ connection string
            var parts = _iotHubOwner.Split(';');
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
