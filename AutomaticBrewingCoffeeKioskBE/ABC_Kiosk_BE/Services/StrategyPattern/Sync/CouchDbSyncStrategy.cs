using CouchDB.Driver;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;
using Domain.CouchDbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.StrategyPattern.Sync
{
    public class CouchDbSyncStrategy<TEntity> : ISyncStrategy<TEntity>
        where TEntity : CouchDocument
    {
        private readonly ILogger<CouchDbSyncStrategy<TEntity>> _logger;
        private readonly CouchDatabase<TEntity> _collection;

        public CouchDbSyncStrategy(ILogger<CouchDbSyncStrategy<TEntity>> logger, KioskDbContext dbContext)
        {
            _logger = logger;
            _collection = dbContext.GetDatabase<TEntity>();
        }

        public async Task DeleteAsync(string entityId, Func<string>? fileFunc = null)
        {
            var entity = await _collection.FindAsync(entityId);
            if (entity != null)
            {
                await _collection.RemoveAsync(entity);
            }
        }

        public async Task<IEnumerable<TEntity>> LoadAllAsync(Func<string>? fileFunc = null)
        {
            return await _collection.ToListAsync();
        }

        public async Task<TEntity?> LoadAsync(string entityId, Func<string>? fileFunc = null)
        {
         return await _collection.FindAsync(entityId);
        }

        public async Task OverwriteAllAsync(IEnumerable<TEntity> entities, Func<string>? fileFunc = null)
        {
            _logger.LogInformation("Overwriting all Device documents in CouchDB...");
            var existingDocs = await _collection.ToListAsync();

            // Xóa từng document
            foreach (var doc in existingDocs)
            {
                await _collection.RemoveAsync(doc);
            }

            // Thêm hoặc cập nhật từng document mới
            foreach (var entity in entities)
            {
                await _collection.AddOrUpdateAsync(entity);
            }
        }

        public Task SaveAsync(TEntity entity, string entityId, Func<string>? fileFunc = null)
        {
            entity.Id = entityId;
            return _collection.AddOrUpdateAsync(entity);
        }
    }
}