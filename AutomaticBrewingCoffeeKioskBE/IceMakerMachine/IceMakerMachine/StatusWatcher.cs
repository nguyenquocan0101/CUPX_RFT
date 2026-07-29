
using IceMakerDevice.Libraries;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Shared.MessageStore;
using Shared.RabbitMqPublisher;

namespace IceMakerMachine
{
    internal class StatusWatcher : BackgroundService
    {
        private readonly IceMachine _machine;
        private readonly string _deviceId;

        private Dictionary<string, object> _deviceStatus = new();
        private Timer? _timer;

        private IRabbitMqPublisher<UpdateStatusStepMsg> _publisher;
        private IRabbitMqPublisher<DeviceLabelMessage> _deviceLabelMsgPublisher;

        public StatusWatcher(IceMachine machine, string deviceId, IRabbitMqPublisher<UpdateStatusStepMsg> publisher, IRabbitMqPublisher<DeviceLabelMessage> deviceLabelMsgPublisher)
        {
            _machine = machine;
            _deviceId = deviceId;
            _publisher = publisher;
            _deviceLabelMsgPublisher = deviceLabelMsgPublisher;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //get query status 1 minute interval
            _timer = new Timer(_ => _ = DoWork(), null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        private async Task DoWork()
        {
            await Program.semaphore.WaitAsync();
            try
            {
                var status = _machine.QueryStatus();
                WriteData(status);
                //publish msg to devicedocument queue
                var msg = new UpdateStatusStepMsg(_deviceId, _deviceStatus);
                var props = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    Type = nameof(UpdateStatusStepMsg)
                };
                await _publisher.PublishMessageAsync(msg, QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY, props);
                Console.WriteLine("Publish device status");

                //publish device label message
                var labels = IceMachine.GetLabels();
                var infoMsg = new DeviceLabelMessage(_deviceId, labels);

                var deviceLabelMessageProps = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Transient,
                    Type = nameof(DeviceLabelMessage)
                };
                await _deviceLabelMsgPublisher.PublishMessageAsync(infoMsg, QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY, deviceLabelMessageProps);
                Console.WriteLine("Publish device label");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Program.semaphore.Release();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return base.StopAsync(cancellationToken);
        }

        private void WriteData(IceMakerStatusCommand statusQuery)
        {
            MapProperties(statusQuery.Data1_FaultStatus);
            _deviceStatus["CurrentSystemStatus"] = statusQuery.Data2_WorkingStatus.ToString();
            if (statusQuery.Data3_AdditionalStatus_Motong != null)
            {
                MapProperties(statusQuery.Data3_AdditionalStatus_Motong); // Prefix for parameters
            }
        }

        #region Reflect Device Status
        void MapProperties<T>(T source, string? prefix = null)
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                var key = $"{prefix ?? string.Empty}{prop.Name}";
                _deviceStatus[key] = prop.GetValue(source);
            }
        }
        #endregion
    }
}
