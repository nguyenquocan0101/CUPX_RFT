using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Services.Interfaces;
using Shared.MessageStore;

namespace Services.DeviceCommands;

public sealed class RabbitMqDeviceMethodInvoker : IDeviceMethodInvoker
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqDeviceMethodInvoker> _logger;

    public RabbitMqDeviceMethodInvoker(
        IConnection connection,
        ILogger<RabbitMqDeviceMethodInvoker> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<DeviceCommandResult> InvokeAsync(
        DeviceCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        using var channel = _connection.CreateModel();
        channel.ExchangeDeclare(QueueConstants.EXCHANGE_DEVICE_COMMAND, ExchangeType.Direct, durable: true, autoDelete: false);
        channel.QueueDeclare(QueueConstants.QUEUE_DEVICE_COMMAND, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx",
                ["x-dead-letter-routing-key"] = QueueConstants.ROUTING_DEVICE_COMMAND,
            });
        channel.QueueBind(QueueConstants.QUEUE_DEVICE_COMMAND, QueueConstants.EXCHANGE_DEVICE_COMMAND, QueueConstants.ROUTING_DEVICE_COMMAND);

        var replyQueue = channel.QueueDeclare(
            queue: string.Empty,
            durable: false,
            exclusive: true,
            autoDelete: true,
            arguments: null).QueueName;
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.CorrelationId = request.CommandId;
        properties.ReplyTo = replyQueue;

        channel.ConfirmSelect();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));
        channel.BasicPublish(
            exchange: QueueConstants.EXCHANGE_DEVICE_COMMAND,
            routingKey: QueueConstants.ROUTING_DEVICE_COMMAND,
            mandatory: false,
            basicProperties: properties,
            body: body);
        if (!channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
            throw new InvalidOperationException($"RabbitMQ did not confirm command {request.CommandId}.");

        var timeout = TimeSpan.FromMilliseconds(Math.Clamp(request.TimeoutMs, 100, 30000));
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = channel.BasicGet(replyQueue, autoAck: false);
            if (response is not null)
            {
                channel.BasicAck(response.DeliveryTag, multiple: false);
                var result = JsonSerializer.Deserialize<DeviceCommandResult>(response.Body.Span);
                if (result is not null && result.CommandId == request.CommandId)
                {
                    return result;
                }
                _logger.LogWarning("Discarded late or mismatched device result for {CommandId}.", request.CommandId);
            }

            await Task.Delay(50, cancellationToken);
        }

        return new DeviceCommandResult(
            request.CommandId,
            request.SchemaVersion,
            request.CorrelationId,
            request.DeviceId,
            "Failed",
            new Dictionary<string, string>(),
            "DEVICE_TIMEOUT",
            $"Device command timed out after {timeout.TotalMilliseconds:0} ms.",
            DateTimeOffset.UtcNow);
    }
}
