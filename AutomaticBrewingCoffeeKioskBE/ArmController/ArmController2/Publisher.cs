using System;
using System.Data.Common;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ArmController2
{
    internal class Publisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public Publisher()
        {
            int port;
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out port) ? port : 5672,
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
            };
            _connection = factory.CreateConnection(); // giữ lại connection
            _channel = _connection.CreateModel();     // tạo channel

            _channel.ExchangeDeclare(QueueConstants.EXCHANGE_NAME, ExchangeType.Direct, durable: true, autoDelete: false);
            _channel.QueueDeclare(QueueConstants.QUEUE_STEP_UPDATE, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueConstants.QUEUE_STEP_UPDATE, QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);

        }

        public void PublishMessage<T>(T message, string exchangeName, string routingKey, string type, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name must be provided.", nameof(exchangeName));

            if (string.IsNullOrWhiteSpace(routingKey))
                throw new ArgumentException("Routing key must be provided.", nameof(routingKey));

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            Console.WriteLine("Push Step State to queue");
            var props = _channel.CreateBasicProperties();
            props.Type = type;
            _channel.BasicPublish(
               exchange: exchangeName,
               routingKey: routingKey,
               mandatory: true,
               basicProperties: props,
               body: body
           );
        }
    }
}
