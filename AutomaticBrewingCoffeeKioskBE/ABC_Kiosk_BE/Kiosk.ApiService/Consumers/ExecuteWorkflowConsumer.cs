
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Repositories.CouchDbRepository;
using Services.Interfaces;
using Shared.MessageStore;
using static Domain.MessageRecords;

namespace Kiosk.ApiService.Consumers
{
    public class ExecuteWorkflowConsumer : BackgroundService
    {
        private readonly ILogger<ExecuteWorkflowConsumer> _logger;
        private IModel _channel;
        private IServiceProvider _provider;
        public ExecuteWorkflowConsumer(IServiceProvider provider, [FromKeyedServices(QueueConstants.QUEUE_WORKFLOW_EXECUTE)] IModel channel, ILogger<ExecuteWorkflowConsumer> logger)
        {
            _logger = logger;
            _channel = channel;
            _provider = provider;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming(QueueConstants.QUEUE_WORKFLOW_EXECUTE, stoppingToken);
            await Task.CompletedTask;
        }

        private void StartConsuming(string queueName, CancellationToken stoppingToken)
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
                if (stoppingToken.IsCancellationRequested) return;
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var success = await HandleBaseOnMesssage(message, ea.BasicProperties.Type, ea.DeliveryTag);
                }

                catch (Exception ex)
                {
                    _logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
                }

            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer); //khởi động quá trình lắng nghe

        }

        private async Task<bool> HandleBaseOnMesssage(string message, string messageType, ulong deliveryTag)
        {
            try
            {
                var jObj = JsonConvert.DeserializeObject<JObject>(message);
                switch (messageType)
                {
                    case nameof(ExecuteCleanWorkflowMsg):
                        {
                            var executeCleanWorkflowMsg = jObj.ToObject<ExecuteCleanWorkflowMsg>();
                            await using var scope = _provider.CreateAsyncScope();
                            var _workflowRepo = scope.ServiceProvider.GetRequiredService<IWorkflowDataRepository>();
                            await _workflowRepo.AddFromCleanWorkflowAsync(executeCleanWorkflowMsg.W, deliveryTag);
                            break;
                        }
                    default: //WorkflowExecuteMsg
                        {
                            await using var scope = _provider.CreateAsyncScope();
                            var _workflowRepo = scope.ServiceProvider.GetRequiredService<IWorkflowDataRepository>();
                            //update couchdb
                            var workflowWithOrderId = GetExecuteWorkflowFromMessage(message);
                            var orderCacheService = scope.ServiceProvider.GetRequiredService<IOrderCacheService>();
                            //check order isFault -> if it is, do not execute workflow
                            var orderInCache = await orderCacheService.GetOrderbyIdAsync(workflowWithOrderId.OrderId);
                            if (orderInCache != null && orderInCache.IsFault)
                            {
                                _logger.LogWarning("Order {OrderId} is faulted. Skipping workflow execution.", workflowWithOrderId.OrderId);
                                //acknowledge the message to remove it from the queue
                                _channel.BasicAck(deliveryTag, false);
                                return true;
                            }
                            await _workflowRepo.AddFromWorkflowAsync(workflowWithOrderId.W, deliveryTag, workflowWithOrderId.Side, workflowWithOrderId.OrderId);

                            break;
                        }

                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private WorkflowExecuteMsg GetExecuteWorkflowFromMessage(string message)
        {
            JObject obj = JObject.Parse(message);
            var workflow = JsonSerializer.CreateDefault().Deserialize<WorkflowExecuteMsg>(new JsonTextReader(new StringReader(message)));
            return workflow ?? throw new ArgumentException("Can not parse workflow");
        }

        private ExecuteCleanWorkflowMsg GetCleanWorkflowFromMessage(string message)
        {
            JObject obj = JObject.Parse(message);
            var workflow = JsonSerializer.CreateDefault().Deserialize<ExecuteCleanWorkflowMsg>(new JsonTextReader(new StringReader(message)));
            return workflow ?? throw new ArgumentException("Can not parse workflow");
        }

        public override void Dispose()
        {
            _channel.Close();
            base.Dispose();
        }
    }
}
