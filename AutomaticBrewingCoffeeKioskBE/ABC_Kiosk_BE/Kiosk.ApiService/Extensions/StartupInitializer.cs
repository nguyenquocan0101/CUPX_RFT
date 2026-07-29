using CouchDB.Driver;
using Domain.CouchDbModels;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Services.Utils;
using Shared.MessageStore;
using Domain;
using Services.Interfaces;

namespace Kiosk.ApiService.Extensions
{
   
    public class RabbitMQSetting
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public interface IStartupInitializer
    {
        Task InitializeAsync();
    }

    public interface IStartupResourceProvisioner
    {
        Task EnsureCouchDatabasesAsync(CancellationToken cancellationToken = default);
        Task EnsureRabbitMqTopologyAsync(CancellationToken cancellationToken = default);
    }

    public sealed class StartupResourceProvisioner : IStartupResourceProvisioner
    {
        public static readonly IReadOnlyList<string> RequiredCouchDatabases =
        [
            nameof(DeviceStatusDocument).GetCouchDbDatabaseNameFormat(),
            nameof(DeviceDocument).GetCouchDbDatabaseNameFormat(),
            nameof(WorkflowData).GetCouchDbDatabaseNameFormat()
        ];

        private readonly IConfiguration _configuration;
        private readonly RabbitMQSetting _rabbitSetting;
        private readonly ILogger<StartupResourceProvisioner> _logger;

        public StartupResourceProvisioner(
            IConfiguration configuration,
            IOptions<RabbitMQSetting> rabbitOptions,
            ILogger<StartupResourceProvisioner> logger)
        {
            _configuration = configuration;
            _rabbitSetting = rabbitOptions.Value;
            _logger = logger;
        }

        public async Task EnsureCouchDatabasesAsync(CancellationToken cancellationToken = default)
        {
            var client = new CouchClient(
                _configuration["CouchDB:Url"]!,
                builder => builder.UseBasicAuthentication(
                    _configuration["CouchDB:Username"]!,
                    _configuration["CouchDB:Pwd"]!));

            await client.GetOrCreateDatabaseAsync<DeviceStatusDocument>(RequiredCouchDatabases[0]);
            await client.GetOrCreateDatabaseAsync<DeviceDocument>(RequiredCouchDatabases[1]);
            await client.GetOrCreateDatabaseAsync<WorkflowData>(RequiredCouchDatabases[2]);
            _logger.LogInformation("Ensured {Count} CouchDB databases.", RequiredCouchDatabases.Count);
        }

        public Task EnsureRabbitMqTopologyAsync(CancellationToken cancellationToken = default)
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitSetting.HostName,
                UserName = _rabbitSetting.UserName,
                Password = _rabbitSetting.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(QueueConstants.EXCHANGE_NAME, ExchangeType.Direct, durable: true, autoDelete: false);

            DeclareQueue(channel, QueueConstants.QUEUE_WORKFLOW_EXECUTE, QueueConstants.QUEUE_WORKFLOW_EXECUTE_ROUTING_KEY);
            DeclareQueue(channel, QueueConstants.QUEUE_STEP_UPDATE, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
            DeclareQueue(channel, QueueConstants.QUEUE_DEVICE_UPDATE, QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY);
            DeclareQueue(channel, QueueConstants.QUEUE_ORDER, QueueConstants.QUEUE_ORDER_ROUTING_KEY_UPDATE);
            DeclareDeviceCommandTopology(channel);

            _logger.LogInformation("Ensured Kiosk RabbitMQ exchange and queues.");
            return Task.CompletedTask;
        }

        private static void DeclareQueue(IModel channel, string queueName, string routingKey)
        {
            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queueName, QueueConstants.EXCHANGE_NAME, routingKey);
        }

        private static void DeclareDeviceCommandTopology(IModel channel)
        {
            var deadLetterExchange = QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx";
            channel.ExchangeDeclare(QueueConstants.EXCHANGE_DEVICE_COMMAND, ExchangeType.Direct, durable: true, autoDelete: false);
            channel.ExchangeDeclare(deadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false);
            channel.QueueDeclare(QueueConstants.QUEUE_DEVICE_COMMAND_DLQ, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(QueueConstants.QUEUE_DEVICE_COMMAND_DLQ, deadLetterExchange, QueueConstants.ROUTING_DEVICE_COMMAND);
            channel.QueueDeclare(
                QueueConstants.QUEUE_DEVICE_COMMAND,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = deadLetterExchange,
                    ["x-dead-letter-routing-key"] = QueueConstants.ROUTING_DEVICE_COMMAND
                });
            channel.QueueBind(QueueConstants.QUEUE_DEVICE_COMMAND, QueueConstants.EXCHANGE_DEVICE_COMMAND, QueueConstants.ROUTING_DEVICE_COMMAND);
        }
    }

    public class StartupInitializer : IStartupInitializer
    {
        private readonly ILogger<StartupInitializer> _logger;
        private readonly IRuntimeStateService _runtimeStateService;

        private readonly IStartupResourceProvisioner _resourceProvisioner;

        public StartupInitializer(
            ILogger<StartupInitializer> logger,
            IRuntimeStateService runtimeStateService,
            IStartupResourceProvisioner resourceProvisioner)
        {
            _logger = logger;
            _runtimeStateService = runtimeStateService;
            _resourceProvisioner = resourceProvisioner;
        }

        public async Task InitializeAsync()
        {
            await _resourceProvisioner.EnsureCouchDatabasesAsync();
            await _resourceProvisioner.EnsureRabbitMqTopologyAsync();
            await SetSystemMaintance();
        }

        private async Task SetSystemMaintance()
        {
            //try get existing value
            var isMaintance = await _runtimeStateService.IsMaintenanceAsync();
            if(!isMaintance)
            {
                _logger.LogInformation("System maintenance mode is set to false.");
                return;
            }
            // Set system maintenance mode in Redis
            await _runtimeStateService.SetMaintenanceAsync(false);
            _logger.LogInformation("System maintenance mode is set to false.");
        }
    }
}
