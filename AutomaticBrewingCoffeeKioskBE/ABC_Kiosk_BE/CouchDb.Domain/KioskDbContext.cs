
using CouchDB.Driver;
using CouchDB.Driver.Options;
using CouchDB.Driver.Types;
namespace Domain.CouchDbModels
{
    public class KioskDbContext : CouchContext
    {
        public KioskDbContext(CouchOptions<KioskDbContext> options) : base(options)
        {
            
        }
        public CouchDatabase<WorkflowData> WorkflowDatas { get; set; }
        public CouchDatabase<DeviceDocument> DeviceDocuments { get; set; }
        public CouchDatabase<DeviceStatusDocument> DeviceStatuses { get; set; }

        public CouchDatabase<TEntity> GetDatabase<TEntity>() where TEntity : CouchDocument
        {
            if (typeof(TEntity) == typeof(DeviceDocument))
                return DeviceDocuments as CouchDatabase<TEntity>;

            if (typeof(TEntity) == typeof(WorkflowData))
                return WorkflowDatas as CouchDatabase<TEntity>;

            if (typeof(TEntity) == typeof(DeviceStatusDocument))
                return DeviceStatuses as CouchDatabase<TEntity>;

            throw new NotSupportedException($"Database for type {typeof(TEntity).Name} is not registered.");
        }

    }
}
