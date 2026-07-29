using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Shared.RabbitMqPublisher
{
    public static class RabbitMqPubExtension
    {
        public static void AddOriginRabitMq(this IServiceCollection services, string hostname, string username, string pwd)
        {
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = hostname,
                    UserName = username,
                    Password = pwd,
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult(); 
            });
            services.AddSingleton(typeof(IRabbitMqPublisher<>), typeof(Publisher<>));
        }

        public static async Task DeclareExchangeWithBindingAsync(this IServiceProvider services, List<ExchangeBindingConfig> configs)
        {
            using var scope = services.CreateScope();
            var conn = scope.ServiceProvider.GetRequiredService<IConnection>();
            await using var channel = await conn.CreateChannelAsync();

            foreach (var config in configs)
            {
                await channel.ExchangeDeclareAsync(config.ExchangeName, config.ExchangeType, durable: true, autoDelete: false);

                foreach (var queue in config.Queues)
                {
                    await channel.QueueDeclareAsync(queue.QueueName, durable: true, exclusive: false, autoDelete: false);
                    await channel.QueueBindAsync(queue.QueueName, config.ExchangeName, queue.RoutingKey);
                }
            }
        }
    }

    public class ExchangeBindingConfig
    {
        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }

        public List<RabbitMqQueue> Queues { get; set; }
    }

    public class RabbitMqQueue(string name, string routingKey)
    {
        public string QueueName { get; set; } = name;
        public string RoutingKey { get; set; } = routingKey;
    }
}
