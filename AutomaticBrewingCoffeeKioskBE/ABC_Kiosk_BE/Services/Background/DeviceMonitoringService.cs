//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Services.Dtos.DeviceMonitoring;
//using Services.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Services.Background
//{
//    public class DeviceMonitoringService : IHostedService, IDisposable
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly IoTHubSettings _settings;
//        private readonly ILogger<DeviceMonitoringService> _logger;
//        private Timer? _timer = null;


//        public DeviceMonitoringService(
//            IServiceProvider serviceProvider,
//            IOptions<IoTHubSettings> settings,
//            ILogger<DeviceMonitoringService> logger)
//        {
//            _serviceProvider = serviceProvider;
//            _settings = settings.Value;
//            _logger = logger;
//        }

//        public Task StartAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogInformation("Timed Hosted Service running.");

//            _timer = new Timer(DoWork, null, TimeSpan.Zero,
//                TimeSpan.FromSeconds(_settings.CheckIntervalSeconds));

//            return Task.CompletedTask;
//        }

//        public Task StopAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogInformation("Timed Hosted Service is stopping.");

//            _timer?.Change(Timeout.Infinite, 0);

//            return Task.CompletedTask;
//        }

//        public void Dispose()
//        {
//            _timer?.Dispose();
//        }

//        private async void DoWork(object? state)
//        {
//            _logger.LogInformation("Device Monitoring Service started. Check interval: {IntervalSeconds} seconds",
//                 _settings.CheckIntervalSeconds);
//            try
//            {
//                await PerformDeviceStatusCheckAsync();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error during device status check cycle");
//            }
//        }

//        private async Task PerformDeviceStatusCheckAsync()
//        {
//            try
//            {
//                using var scope = _serviceProvider.CreateScope();
//                var iotHubService = scope.ServiceProvider.GetRequiredService<IIoTHubService>();
//                var deviceStatusService = scope.ServiceProvider.GetRequiredService<IDeviceService>();

//                _logger.LogDebug("Starting device status check cycle");

//                var deviceStatuses = await iotHubService.CheckDevicesConnectionStatusAsync();

//                foreach (var deviceStatus in deviceStatuses)
//                {
//                    await deviceStatusService.UpdateDeviceStatusAsync(deviceStatus);
//                }

//                _logger.LogInformation("Device status check completed. Total devices: {DeviceCount}",
//                    deviceStatuses.Count());
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error performing device status check");
//            }
//        }

//    }
//}
