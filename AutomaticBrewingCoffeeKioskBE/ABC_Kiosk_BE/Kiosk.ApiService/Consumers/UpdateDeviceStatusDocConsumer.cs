using System.Text;
using CouchDB.Driver;
using Domain.CouchDbModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Shared.MessageStore;

namespace Kiosk.ApiService.Consumers
{
    public class UpdateDeviceStatusDocConsumer : BackgroundService
    {
        private readonly ILogger<UpdateDeviceStatusDocConsumer> _logger;
        private IModel _channel;
        private readonly ICouchDatabase<DeviceStatusDocument> _deviceStatusDb;
        public UpdateDeviceStatusDocConsumer(KioskDbContext db, [FromKeyedServices(QueueConstants.QUEUE_DEVICE_UPDATE)] IModel channel, ILogger<UpdateDeviceStatusDocConsumer> logger)
        {
            _deviceStatusDb = db.DeviceStatuses;
            _logger = logger;
            _channel = channel;
            //_provider = provider;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming(QueueConstants.QUEUE_DEVICE_UPDATE, stoppingToken);
            await Task.CompletedTask;
        }

        private void StartConsuming(string queueName, CancellationToken cancellationToken)
        {
            //just consume the queue if it exists
            try
            {
                _channel.QueueDeclarePassive(queue: queueName);
            }
            catch (OperationInterruptedException)
            {
                return;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    string type = ea.BasicProperties.Type;
                    await HandleMessage(message, type);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer); //khởi động quá trình lắng nghe

        }

        private async Task<bool> HandleMessage(string message, string messageType)
        {
            try
            {
                var jObj = JsonConvert.DeserializeObject<JObject>(message);

                switch (messageType)
                {
                    case nameof(UpdateStatusStepMsg):
                        var statusMsg = jObj.ToObject<UpdateStatusStepMsg>();

                        DeviceStatusDocument? deviceToUpdateStatus = await _deviceStatusDb.FindAsync(statusMsg.DeviceId);
                        if(deviceToUpdateStatus == null)
                        {
                            _logger.LogInformation($"Device status document not found for DeviceId: {statusMsg.DeviceId}. Creating a new document for device status.");
                            deviceToUpdateStatus = new DeviceStatusDocument
                            {
                                Id = statusMsg.DeviceId,
                                DeviceId = statusMsg.DeviceId,
                                Status = statusMsg.status ?? [],
                                LastUpdated = DateTime.Now,
                            };
                        }
                        else
                        {
                            _logger.LogInformation($"Updating existing device status document for DeviceId: {statusMsg.DeviceId}.");
                            deviceToUpdateStatus.Status = statusMsg.status;
                            deviceToUpdateStatus.LastUpdated = DateTime.Now;
                        }
                        await _deviceStatusDb.AddOrUpdateAsync(deviceToUpdateStatus);
                        break;

                    case nameof(DeviceLabelMessage):
                        var deviceLabelMessage = jObj.ToObject<DeviceLabelMessage>();
                        var deviceToUpdateLabel = await _deviceStatusDb.FindAsync(deviceLabelMessage.DeviceId);
                        if (deviceToUpdateLabel == null)
                        {
                            _logger.LogInformation($"Device status document not found for DeviceId: {deviceLabelMessage.DeviceId}. Creating a new document for label.");
                            deviceToUpdateLabel = new DeviceStatusDocument
                            {
                                Id = deviceLabelMessage.DeviceId,
                                DeviceId = deviceLabelMessage.DeviceId,
                                Labels = deviceLabelMessage.Labels ?? new Dictionary<string, string>(),
                                LastUpdated = DateTime.Now,
                            };
                        }else
                        {
                            _logger.LogInformation($"Updating existing device label for DeviceId: {deviceLabelMessage.DeviceId}.");
                            deviceToUpdateLabel.Labels = deviceLabelMessage.Labels;
                            deviceToUpdateLabel.LastUpdated = DateTime.Now;
                        }
                        await _deviceStatusDb.AddOrUpdateAsync(deviceToUpdateLabel);
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Some errors happen. Error: {e.Message}");
                return false;
            }

        }

        public override void Dispose()
        {
            _channel.Close();

            base.Dispose();
        }
    }
}
