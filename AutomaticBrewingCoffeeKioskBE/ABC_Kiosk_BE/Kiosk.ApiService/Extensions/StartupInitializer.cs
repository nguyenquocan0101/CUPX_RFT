using CouchDB.Driver;
using Domain.CouchDbModels;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Services.Utils;
using Shared.MessageStore;
using CouchDB.Driver.Exceptions;
using StackExchange.Redis;
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

    public class StartupInitializer : IStartupInitializer
    {
        private readonly ILogger<StartupInitializer> _logger;
        private readonly IConfiguration _configuration;
        private readonly RabbitMQSetting _rabbitSetting;
        private readonly IRuntimeStateService _runtimeStateService;

        public StartupInitializer(ILogger<StartupInitializer> logger, IOptions<RabbitMQSetting> option, IConfiguration configuration, IRuntimeStateService runtimeStateService)
        {
            _logger = logger;
            _rabbitSetting = option.Value;
            _configuration = configuration;
            _runtimeStateService = runtimeStateService;
        }

        public async Task InitializeAsync()
        {
            await CreateCouchDb(nameof(DeviceStatusDocument).GetCouchDbDatabaseNameFormat());
            await DeleteCouchDbDatabase(nameof(WorkflowData).GetCouchDbDatabaseNameFormat());
            DeleteRabbitMqQueue(QueueConstants.QUEUE_WORKFLOW_EXECUTE);
            DeleteRabbitMqQueue(QueueConstants.QUEUE_STEP_UPDATE);
            await SetSystemMaintance();
        }

        private async Task CreateCouchDb(string databaseName)
        {
            var url = _configuration["CouchDB:Url"]!;
            var username = _configuration["CouchDB:Username"]!;
            var pwd = _configuration["CouchDB:Pwd"]!;
            var client = new CouchClient(url,
              builder => builder.UseBasicAuthentication(username, pwd));
            try
            {
                await client.GetOrCreateDatabaseAsync<DeviceStatusDocument>(databaseName);
                _logger.LogInformation("Create CouchDB database: {Database}", databaseName);
            }
            catch (CouchException ce)
            {
                _logger.LogWarning("Failed to create CouchDB database: {Database}. Error: {Status}", databaseName, ce.Message);
            }
        }

        private async Task DeleteCouchDbDatabase(string databaseName)
        {
            var url = _configuration["CouchDB:Url"]!;
            var username = _configuration["CouchDB:Username"]!;
            var pwd = _configuration["CouchDB:Pwd"]!;

            var client = new CouchClient(url,
                builder => builder.UseBasicAuthentication(username, pwd));
            try
            {
                await client.DeleteDatabaseAsync(databaseName);
                _logger.LogInformation("Deleted CouchDB database: {Database}", databaseName);
            }
            catch (CouchException ce)
            {
                _logger.LogWarning("Failed to delete CouchDB database: {Database}. Error: {Status}", databaseName, ce.Message);
            }
        }

        private void DeleteRabbitMqQueue(string queueName)
        {
            var factory = new ConnectionFactory() {
                HostName = _rabbitSetting.HostName,
                UserName = _rabbitSetting.UserName,
                Password = _rabbitSetting.Password,
            }; 
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            try
            {
                channel.QueueDelete(queueName, ifUnused: false, ifEmpty: false);
                _logger.LogInformation("Deleted RabbitMQ queue: {Queue}", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete RabbitMQ queue: {Queue}", queueName);
            }
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
