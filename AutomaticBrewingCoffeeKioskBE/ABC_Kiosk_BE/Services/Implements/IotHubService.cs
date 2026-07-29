using AutoMapper;
using Confluent.Kafka;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repositories.Interfaces;
using Services.Dtos.DeviceMonitoring;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class IotHubService : BaseService<IotHubService>, IIoTHubService

    {
        private readonly RegistryManager _registryManager;
        private readonly IoTHubSettings _settings;

        public IotHubService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor, IOptions<IoTHubSettings> settings) : base(unitOfWork, mapper, loggerFactory, httpContextAccessor)
        {
            _settings = settings.Value;
            try
            {
                _registryManager = RegistryManager.CreateFromConnectionString(_settings.ConnectionString);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Dtos.DeviceMonitoring.DeviceStatus>> CheckDevicesConnectionStatusAsync()
        {
            var deviceStatuses = new List<Dtos.DeviceMonitoring.DeviceStatus>();

            try
            {
                var deviceIds = await GetRegisteredDeviceIdsAsync();
                var tasks = deviceIds.Select(CheckSingleDeviceStatusAsync);
                var results = await Task.WhenAll(tasks);
                deviceStatuses.AddRange(results);
                _logger.LogInformation("Checked status for {DeviceCount} devices", deviceStatuses.Count);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking devices connection status");
            }

            return deviceStatuses;

        }

        private async Task<Dtos.DeviceMonitoring.DeviceStatus> CheckSingleDeviceStatusAsync(string deviceId)
        {
            var deviceStatus = new Dtos.DeviceMonitoring.DeviceStatus
            {
                DeviceId = deviceId,
                LastChecked = DateTime.UtcNow,
                IsConnected = false,
            };

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeoutSeconds));

                var device = await _registryManager.GetDeviceAsync(deviceId, cts.Token);
                if (device != null)
                {
                    deviceStatus.DeviceName = device.Id;
                    deviceStatus.ConnectionState = device.ConnectionState.ToString();
                    deviceStatus.IsConnected = device.ConnectionState == DeviceConnectionState.Connected;
                    deviceStatus.LastSeen = device.LastActivityTime;
                    deviceStatus.StatusMessage = $"Status: {device.Status}, State: {device.ConnectionState}";
                }
                else
                {
                    deviceStatus.StatusMessage = "Device not found";
                    deviceStatus.ConnectionState = EDeviceConnectionState.Unknown.ToString();
                }
            }
            catch (OperationCanceledException)
            {
                deviceStatus.StatusMessage = "Check timeout";
                deviceStatus.ConnectionState = EDeviceConnectionState.Error.ToString();
                _logger.LogWarning("Timeout checking device {DeviceId}", deviceId);
            }
            catch (Exception ex)
            {
                deviceStatus.StatusMessage = $"Error: {ex.Message}";
                deviceStatus.ConnectionState = EDeviceConnectionState.Error.ToString();
                _logger.LogError(ex, "Error checking device {DeviceId}", deviceId);
            }

            return deviceStatus;
        }

        private async Task<IEnumerable<string>> GetRegisteredDeviceIdsAsync()
        {
            var deviceIds = new List<string>();

            try
            {
                var query = _registryManager.CreateQuery("SELECT * FROM devices");

                while (query.HasMoreResults)
                {
                    IEnumerable<Twin> twins = await query.GetNextAsTwinAsync();
                    deviceIds.AddRange(twins.Select(twin => twin.DeviceId));
                }

                _logger.LogDebug("Found {DeviceCount} registered devices", deviceIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving registered device IDs");
            }

            return deviceIds;
        }
    }
}
