using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs.Consumer;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.Dtos.ArmMachine;
using Services.WebSockets;

namespace Services.Background
{
    public class D2CMsgReceivingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<D2CMsgReceivingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _kioskId;
        private readonly EventHubConsumerClient eventHubConsumerClient;
        private readonly IWebSocketManager _webSocketManager;

        public D2CMsgReceivingService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<D2CMsgReceivingService> logger,
            IWebSocketManager webSocketManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            eventHubConsumerClient = new EventHubConsumerClient(
                _configuration["AzureEventHub:ConsumerGroup"]!,
                _configuration["AzureEventHub:ConnString"]!);
            _kioskId = _configuration["KioskId"]!;
            _webSocketManager = webSocketManager;         
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("D2C Message Receiving Service started.");

            using var scope = _serviceProvider.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
            var db = scope.ServiceProvider.GetRequiredService<AutoBrewingKioskBeContext>();
            await ReceiveD2CMesssage(cache, db, stoppingToken);
        }

        private async Task ReceiveD2CMesssage(IDistributedCache cache, AutoBrewingKioskBeContext db, CancellationToken stoppingToken)
        {
            string? lastMsgOffset = await cache.GetStringAsync("lastMsgOffset", stoppingToken);
            bool allowRead = string.IsNullOrEmpty(lastMsgOffset);

            await foreach (PartitionEvent evt in eventHubConsumerClient.ReadEventsAsync(stoppingToken))
            {
               // _logger.LogInformation($"Received event from partition {evt.Partition.PartitionId} at {evt.Data.EnqueuedTime.UtcDateTime}");
                if (stoppingToken.IsCancellationRequested) break;

                var data = evt.Data;
                string msgOffset = data.OffsetString;

                // Nếu chưa cho phép đọc, kiểm tra offset checkpoint
                if (!allowRead)
                {
                    if (msgOffset == lastMsgOffset)
                    {
                        allowRead = true;
                        _logger.LogInformation("Checkpoint found. Allow reading next messages...");
                        continue;
                    }
                    else
                    {
                        continue;
                    }
                }

                var propertyObject = data.Properties;
                string body = Encoding.UTF8.GetString(data.Body.ToArray());
                ArmCoordinateResponse armData = new ArmCoordinateResponse();
                armData = JsonSerializer.Deserialize<ArmCoordinateResponse>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var deviceId = data.SystemProperties.TryGetValue("iothub-connection-device-id", out object devIdObj) ? devIdObj.ToString() : null;
                var kioskId = propertyObject.TryGetValue("kioskId", out object kioskIdObj) ? kioskIdObj.ToString() : null;
                var flag = propertyObject.TryGetValue("flag", out object flagObj) ? flagObj.ToString() : null;

                if (kioskId == _kioskId && !string.IsNullOrEmpty(msgOffset) && msgOffset != lastMsgOffset)
                {     
                    _logger.LogInformation($"[Kiosk:{_kioskId}] Received: {body} (Offset: {msgOffset})");            

                    // Ghi offset mới nhất vào cache
                    await cache.SetStringAsync("lastMsgOffset", msgOffset, stoppingToken);
                    lastMsgOffset = msgOffset;

                    if(flag == "ws")
                    {
                        _webSocketManager.SendMessageToAllAsync(JsonSerializer.Serialize(armData.Coordinate));
                    } else
                    {
                        // Lưu database
                        db.DeviceLogs.Add(new DeviceLog
                        {
                            DeviceId = deviceId,
                            LogKey = armData.InformationType,
                            LogValue = armData.InformationType,
                            LogType = GetDeviceLogType(armData.InformationType),
                            CreatedAt = DateTime.UtcNow
                        });
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
            }
        }

        private DeviceLogType GetDeviceLogType(string logType)
        {
            switch (logType?.Trim().ToLowerInvariant())
            {
                case "information":
                case "info":
                    return DeviceLogType.Info;
                case "error":
                    return DeviceLogType.Error;
                default:
                    return DeviceLogType.Unknown;
            }
        }
    }
}