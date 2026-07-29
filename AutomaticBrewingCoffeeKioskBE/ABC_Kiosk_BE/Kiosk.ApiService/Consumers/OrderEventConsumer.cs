
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Services.Interfaces;
using Shared.MessageStore;
using static Domain.MessageRecords;

namespace Kiosk.ApiService.Consumers
{
    public class OrderEventConsumer : BackgroundService, IDisposable
    {
        private readonly IServiceProvider _provider;
        private readonly IModel _orderChannel;
        private readonly ILogger<OrderEventConsumer> _logger;
        public OrderEventConsumer(IServiceProvider provider, [FromKeyedServices(QueueConstants.QUEUE_ORDER)] IModel orderChannel, ILogger<OrderEventConsumer> logger)
        {
            _provider = provider;
            _orderChannel = orderChannel;
            _logger = logger;

        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming(QueueConstants.QUEUE_ORDER, stoppingToken);
            await Task.CompletedTask;
        }

        private void StartConsuming(string queueName, CancellationToken cancellationToken)
        {
            //just consume the queue if it exists
            try
            {
                _orderChannel.QueueDeclarePassive(queue: queueName);
            }
            catch (OperationInterruptedException)
            {
                _logger.LogWarning($"No {queueName} queue is declared");
                return;
            }

            var consumer = new EventingBasicConsumer(_orderChannel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                bool processedSuccessfully = false;
                try
                {
                    string type = ea.BasicProperties.Type;
                    processedSuccessfully = await DoBasedOnMessage(message, type);
                    _logger.LogInformation($"Check order completed. Result: {processedSuccessfully}");


                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception occurred while processing message from queue {queueName}: {ex}");
                }

                _orderChannel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _orderChannel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer); //khởi động quá trình lắng nghe

        }

        private async Task<bool> DoBasedOnMessage(string message, string messageType)
        {
            try
            {
                bool result = false;
                using var scope = _provider.CreateScope();
                var orderCacheService = scope.ServiceProvider.GetRequiredService<IOrderCacheService>();
                var jObj = JsonConvert.DeserializeObject<JObject>(message);
                switch (messageType)
                {
                    case nameof(FinishProductMsg):
                        var finishProductMsg = jObj.ToObject<FinishProductMsg>();
                        var parsed = DateTime.TryParse(finishProductMsg.FinishTime, out DateTime fnTime);
                        var orderInCache = await orderCacheService.UpdateFinishProductInOrderAsync(
                        finishProductMsg.OrderId,
                                finishProductMsg.ProductId,
                                parsed ? fnTime : DateTime.Now
                            );

                        if (orderInCache != null)
                        {
                            _logger.LogInformation(
                                "Order {OrderId} with finished product {ProductId} updated successfully",
                                orderInCache.OrderId,
                                finishProductMsg.ProductId
                            );
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Failed to update finished product {ProductId} in order {OrderId}",
                                finishProductMsg.ProductId,
                                finishProductMsg.OrderId
                            );
                            break; // exit if update fail
                        }

                        // Now check if all products are finished 
                        if (await orderCacheService.IsOrderCompleteAsync(finishProductMsg.OrderId))
                        {
                            _logger.LogInformation("Order {OrderId} is finished. Updating order complete status to cloud.", finishProductMsg.OrderId);

                            var orderInCacheComplete = await orderCacheService.GetOrderbyIdAsync(finishProductMsg.OrderId);
                            var idList = orderInCacheComplete.Products.Select(p => p.ProductId).ToList();

                            result = await orderCacheService.UpdateCompleteOrderToCloudAsync(finishProductMsg.OrderId, idList);
                            if (!result)
                                _logger.LogWarning("Failed to update order {OrderId} to cloud", finishProductMsg.OrderId);
                            else
                                _logger.LogInformation("Order {OrderId} successfully updated to cloud", finishProductMsg.OrderId);
                        }
                        else
                        {
                            _logger.LogInformation("Order {OrderId} is not finished yet. Continue order process", finishProductMsg.OrderId);
                        }

                        break;
                    case nameof(FailProductMsg):
                        var failProductMsg = jObj.ToObject<FailProductMsg>();
                        bool canParsed = DateTime.TryParse(failProductMsg.FailTime, out DateTime failTime);
                        var orderInCacheToFail = await orderCacheService.UpdateFailProductInOrderAsync(
                                failProductMsg.OrderId,
                                failProductMsg.ProductId,
                                canParsed ? failTime : DateTime.Now
                            );

                        if (orderInCacheToFail != null)
                        {
                            _logger.LogInformation(
                                "Order {OrderId} with failed product {ProductId} updated successfully",
                                orderInCacheToFail.OrderId,
                                failProductMsg.ProductId
                            );
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Failed to update failed product {ProductId} in order {OrderId}",
                                failProductMsg.ProductId,
                                failProductMsg.OrderId
                            );
                            break; // exit if update fail
                        }
                        //get finishedProductIds, failedProductIds, preparingProductIds
                        var finishedProductIds = new List<string>();
                        var failedProductIds = new List<string>();
                        var preparingProductIds = new List<string>();

                        foreach (var p in orderInCacheToFail.Products)
                        {
                            if (p.FinishTime != null)
                                finishedProductIds.Add(p.ProductId);
                            else if (p.FailTime != null)
                                failedProductIds.Add(p.ProductId);
                            else 
                                preparingProductIds.Add(p.ProductId);
                        }

                        //call cloud to update order failed
                        result = await orderCacheService.UpdateFailedOrderToCloudAsync(failProductMsg.OrderId, failProductMsg.Message, finishedProductIds, failedProductIds, preparingProductIds);

                        break;
                }
                return result;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public override void Dispose()
        {
            _orderChannel.Close();
            base.Dispose();
        }
    }
}
