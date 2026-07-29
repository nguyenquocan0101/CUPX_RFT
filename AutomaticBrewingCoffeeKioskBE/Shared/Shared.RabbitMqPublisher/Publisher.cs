using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Shared.RabbitMqPublisher
{
    public interface IRabbitMqPublisher<T> where T : class
    {
        Task PublishMessageAsync(T message, string exchangeName, string routingKey, CancellationToken cancellationToken = default);
        Task PublishMessageAsync(T message, string exchangeName, string routingKey, BasicProperties props, CancellationToken cancellationToken = default);

    }

    public class Publisher<T> : IRabbitMqPublisher<T> where T : class
    {
        private readonly IConnection _conn;
        public Publisher(IConnection conn)
        {
            _conn = conn;
        }
        public async Task PublishMessageAsync(T message, string exchangeName, string routingKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name must be provided.", nameof(exchangeName));

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("Routing key must be provided.", nameof(routingKey));

            await using var channel = await _conn.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent, //msg được lưu vào disk để bảo toàn
            };

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken
            );
        }


        public async Task PublishMessageAsync(T message, string exchangeName, string routingKey, BasicProperties props, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name must be provided.", nameof(exchangeName));

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("Routing key must be provided.", nameof(routingKey));

            await using var channel = await _conn.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken
            );
        }
    }
}
