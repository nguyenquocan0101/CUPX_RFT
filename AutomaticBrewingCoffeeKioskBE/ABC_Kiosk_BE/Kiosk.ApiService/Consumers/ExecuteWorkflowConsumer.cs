
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repositories.CouchDbRepository;
using Services.Interfaces;
using Shared.MessageStore;
using static Domain.MessageRecords;

namespace Kiosk.ApiService.Consumers
{
    public class ExecuteWorkflowConsumer : BackgroundService
    {
        private readonly ILogger<ExecuteWorkflowConsumer> _logger;
        private readonly IModel _channel;
        private readonly IWorkflowDeliveryTracker _deliveryTracker;
        private IServiceProvider _provider;
        public ExecuteWorkflowConsumer(IServiceProvider provider, [FromKeyedServices(QueueConstants.QUEUE_WORKFLOW_EXECUTE)] IModel channel, ILogger<ExecuteWorkflowConsumer> logger, IWorkflowDeliveryTracker deliveryTracker)
        {
            _logger = logger;
            _channel = channel;
            _provider = provider;
            _deliveryTracker = deliveryTracker;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                StartConsuming(QueueConstants.QUEUE_WORKFLOW_EXECUTE, stoppingToken);
                _logger.LogInformation("Workflow execute consumer started on {QueueName}.", QueueConstants.QUEUE_WORKFLOW_EXECUTE);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Workflow execute consumer stopped during startup.");
                throw;
            }
        }

        private void StartConsuming(string queueName, CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                if (stoppingToken.IsCancellationRequested) return;
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    _deliveryTracker.Register(ea.DeliveryTag);
                    var success = await HandleBaseOnMesssage(message, ea.BasicProperties.Type, ea.DeliveryTag);
                    if (!success)
                    {
                        _deliveryTracker.TryTake(ea.DeliveryTag);
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                        _logger.LogWarning("Dropped invalid workflow message from queue {QueueName}.", queueName);
                    }
                }

                catch (Exception ex)
                {
                    _deliveryTracker.TryTake(ea.DeliveryTag);
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    _logger.LogError(ex, "Exception occurred while processing message from queue {QueueName}.", queueName);
                }

            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer); //khởi động quá trình lắng nghe
            _logger.LogInformation("Workflow execute consumer subscribed to {QueueName}.", queueName);

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
                                _deliveryTracker.TryTake(deliveryTag);
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
