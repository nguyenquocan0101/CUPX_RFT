using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Shared.MessageStore;
using Shared.RabbitMqPublisher;
using static CoffeeMachineController.SlaveStatusCommand;

namespace CoffeeMachineController
{
    internal class StatusWatcher : BackgroundService
    {
        private readonly CoffeeMachine _cf;
        private readonly string _deviceId;

        private Dictionary<string, object> _deviceStatus = new();
        private Timer? _timer;

        private IRabbitMqPublisher<UpdateStatusStepMsg> _publisher;
        private IRabbitMqPublisher<DeviceLabelMessage> _deviceLabelMsgPublisher;
        public StatusWatcher(CoffeeMachine cf, string deviceId, IRabbitMqPublisher<UpdateStatusStepMsg> publisher, IRabbitMqPublisher<DeviceLabelMessage> deviceLabelMsgPublisher)
        {
            _cf = cf;
            _deviceId = deviceId;
            //_deviceStatus = GetStatusDataFormat();

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
                //publish device status message
                var status = _cf.QueryStatus();
                WriteData(status);
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
                var labels = CoffeeMachine.GetLabels();
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


        private void WriteData(SlaveStatusCommand statusQuery)
        {
            MapProperties(statusQuery.Data1);
            MapProperties(statusQuery.Data2);
            _deviceStatus["CurrentSystemStatus"] = statusQuery.Data1.CurrentSystemStatus.ToString();
            MapProperties(statusQuery.Data3);
            MapProperties(statusQuery.Data4);
            MapProperties(statusQuery.ProductionProgress);
            MapProperties(statusQuery.Data6);
            MapProperties(statusQuery.Data7);
            MapProperties(statusQuery.Data8);
            MapProperties(statusQuery.Data9);
            MapProperties(statusQuery.Data10);
            MapProperties(statusQuery.Data11);
            MapProperties(statusQuery.Data12);
            MapProperties(statusQuery.Data13);
            MapProperties(statusQuery.Data14);
            MapProperties(statusQuery.Data15);
            MapProperties(statusQuery.Data16);
            MapProperties(statusQuery.Data17);
            MapProperties(statusQuery.Data18);
            MapProperties(statusQuery.Data19);
            MapProperties(statusQuery.Data20);
            MapProperties(statusQuery.Data21);
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
