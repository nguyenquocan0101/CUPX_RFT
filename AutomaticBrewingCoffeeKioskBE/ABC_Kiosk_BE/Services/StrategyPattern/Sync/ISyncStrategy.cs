using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.StrategyPattern.Sync
{
    public interface ISyncStrategy<TEntity> where TEntity : class
    {
        Task OverwriteAllAsync(IEnumerable<TEntity> entities, Func<string>? fileFunc = null);
        Task SaveAsync (TEntity entity, string entityId, Func<string>? fileFunc = null);
        Task<TEntity?> LoadAsync(string entityId, Func<string>? fileFunc = null);
        Task DeleteAsync(string entityId, Func<string>? fileFunc = null);
        Task<IEnumerable<TEntity>> LoadAllAsync(Func<string>? fileFunc = null);
    }
}
