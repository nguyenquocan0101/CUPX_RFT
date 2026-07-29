using Domain;
using Flurl.Util;
using Newtonsoft.Json.Linq;
using Services.Interfaces;
using StackExchange.Redis;

namespace Services.Implements
{

    public class RedisRuntimeStateService : IRuntimeStateService
    {
        private const string Key = SharedValues.KIOSK_SYSTEM_MAINTANCE_KEY;
        private readonly IDatabase _db;

        public RedisRuntimeStateService(IDatabase database)
        {
            _db = database;
        }

        public async Task SetMaintenanceAsync(bool on)
        {
            var value = on ? SharedValues.KIOSK_SYSTEM_MAINTANCE_TRUE : SharedValues.KIOSK_SYSTEM_MAINTANCE_FALSE;
            await _db.StringSetAsync(Key, value);
        }

        public async Task<bool> IsMaintenanceAsync()
        {
            RedisValue str = await _db.StringGetAsync(Key);
            
            if (!str.HasValue)
            {

                await _db.StringSetAsync(Key, SharedValues.KIOSK_SYSTEM_MAINTANCE_FALSE); 
                return false;
            }

            return str.ToString() == SharedValues.KIOSK_SYSTEM_MAINTANCE_TRUE;
        }
    }

}
