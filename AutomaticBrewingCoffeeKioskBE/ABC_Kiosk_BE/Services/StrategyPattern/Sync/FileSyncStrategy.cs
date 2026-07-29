using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.StrategyPattern.Sync
{
    public class FileSyncStrategy<TEntity> : ISyncStrategy<TEntity> where TEntity : class
    {
        private  string _entityDirectory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<FileSyncStrategy<TEntity>> _logger;
        private readonly bool _preserveLocalWorkflowFixtures;

        public FileSyncStrategy(
            IWebHostEnvironment webHostEnvironment,
            ILogger<FileSyncStrategy<TEntity>> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _preserveLocalWorkflowFixtures = string.Equals(
                configuration["LOCAL_MODE"],
                "true",
                StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    configuration["ASPNETCORE_ENVIRONMENT"],
                    "Local",
                    StringComparison.OrdinalIgnoreCase);
            var dataDirectory = Path.Combine(webHostEnvironment.ContentRootPath, "DataStorage");
            _entityDirectory = Path.Combine(dataDirectory, typeof(TEntity).Name);
            Directory.CreateDirectory(_entityDirectory);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public Task DeleteAsync(string entityId, Func<string>? fileFunc = null)
        {
            if (fileFunc != null)
            {
                _entityDirectory = fileFunc();
            }
            var filePath = Path.Combine(_entityDirectory, $"{entityId}.json");
            if (IsProtectedLocalWorkflowFixture(filePath))
            {
                _logger.LogDebug("Preserved local workflow fixture {EntityId}.", entityId);
                return Task.CompletedTask;
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogDebug($"Deleted {typeof(TEntity).Name} with ID {entityId} from file.");
            }
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<TEntity>> LoadAllAsync(Func<string>? fileFunc = null)
        {
            var entities = new List<TEntity>();
            if (fileFunc != null)
            {
                _entityDirectory = fileFunc();
            }
            if (!Directory.Exists(_entityDirectory)) return entities;
            var files = Directory.GetFiles(_entityDirectory, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(file);
                    var entity = JsonSerializer.Deserialize<TEntity>(jsonContent, _jsonOptions);
                    if (entity != null) entities.Add(entity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error loading entity from file {file}");
                }
            }
            return entities;
        }

        public async Task<TEntity?> LoadAsync(string entityId, Func<string>? fileFunc = null)
        {
            if (fileFunc != null)
            {
                _entityDirectory = fileFunc();
            }
            var filePath = Path.Combine(_entityDirectory, $"{entityId}.json");
            if (!File.Exists(filePath)) return null;

            var jsonContent = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<TEntity>(jsonContent, _jsonOptions);
        }

        public async Task OverwriteAllAsync(IEnumerable<TEntity> entities, Func<string>? fileFunc = null)
        {
            _logger.LogInformation($"Overwriting all data for {typeof(TEntity).Name} in '{_entityDirectory}'...");
        if (fileFunc != null)
            {
                _entityDirectory = fileFunc();
            }
            if (Directory.Exists(_entityDirectory))
            {
                var directoryInfo = new DirectoryInfo(_entityDirectory);
                foreach (var file in directoryInfo.GetFiles())
                {
                    if (IsProtectedLocalWorkflowFixture(file.FullName)) continue;
                    file.Delete();
                }
            }
            foreach (var entity in entities)
            {
                string entityType = typeof(TEntity).Name;
                var keyPropertyName = entityType + "Id";
                if(entityType == nameof(Workflow))
                {
                    keyPropertyName = nameof(Workflow.ProductId);
                }

                var entityId = string.Empty;
                var preCheckId = entity.GetType().GetProperty(keyPropertyName)?.GetValue(entity)?.ToString();
                if (string.IsNullOrEmpty(preCheckId))
                {
                    entityId = entity.GetType().GetProperty(entityType + "Id")?.GetValue(entity)?.ToString();
                } else
                {
                    entityId = preCheckId;
                }
                
                if (entityId == null)
                {
                    _logger.LogWarning($"Entity of type {typeof(TEntity).Name} does not have an Id property.");
                    continue;
                }
                await SaveAsync(entity, entityId, fileFunc);
            }
        }

        public async Task SaveAsync(TEntity entity, string entityId, Func<string>? fileFunc = null)
        {
            if (fileFunc != null)
            {
                _entityDirectory = fileFunc();
            }
           var filePath = Path.Combine(_entityDirectory, $"{entityId}.json");
            Directory.CreateDirectory(_entityDirectory);
            try
            {
                var json = JsonSerializer.Serialize(entity, _jsonOptions);
                File.WriteAllText(filePath, json);
                _logger.LogInformation($"Entity {entityId} saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving entity {entityId} to file.");
                throw;
            }
        }

        private bool IsProtectedLocalWorkflowFixture(string filePath)
        {
            return _preserveLocalWorkflowFixtures
                && typeof(TEntity) == typeof(Workflow)
                && Path.GetFileNameWithoutExtension(filePath)
                    .StartsWith("local-", StringComparison.OrdinalIgnoreCase);
        }
    }
}
