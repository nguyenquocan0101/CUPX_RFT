using System.Text.Json;
using Services.Interfaces;
using StackExchange.Redis;

namespace Services.Redis;

public class RedisService : IRedisService
{
    private IDatabase _cacheDb;
    private IConnectionMultiplexer _redis;

    // Lua - INCR và nếu lần đầu thì EXPIRE
    private const string IncrWithExpireScript = @"
local val = redis.call('INCR', KEYS[1])
if val == 1 then
  redis.call('EXPIRE', KEYS[1], ARGV[1])
end
return val";

    // Lua - INCRBY và nếu lần đầu (val == inc) thì EXPIRE
    private const string IncrByWithExpireScript = @"
local val = redis.call('INCRBY', KEYS[1], ARGV[1])
if val == tonumber(ARGV[1]) then
  redis.call('EXPIRE', KEYS[1], ARGV[2])
end
return val";

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        if (redis.IsConnected)
        {
            _cacheDb = redis.GetDatabase();
        }
    }

    public async Task<T?> GetDataAsync<T>(string key)
    {
        var value = await _cacheDb.StringGetAsync(key);
        if (!value.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<T>(value.ToString());
        }

        return default;
    }

    public async Task<bool> SetDataAsync<T>(string key, T value, DateTimeOffset? expirationTime = null)
    {
        if (!_redis.IsConnected)
        {
            return false;
        }

        TimeSpan? expiryTime = expirationTime.HasValue
            ? expirationTime.Value - DateTimeOffset.Now
            : (TimeSpan?)null;

        return await _cacheDb.StringSetAsync(key, JsonSerializer.Serialize(value), expiryTime);
    }

    public async Task<bool> RemoveDataAsync(string key)
    {
        if (!_redis.IsConnected)
        {
            return false;
        }

        var exist = _cacheDb.KeyExists(key);
        if (exist)
        {
            return await _cacheDb.KeyDeleteAsync(key);
        }

        return false;
    }

    public async Task<long> IncrWithExpireAsync(string key, TimeSpan ttl)
    {
        var result = await _cacheDb.ScriptEvaluateAsync(
            IncrWithExpireScript, // <= string
            new RedisKey[] { (RedisKey)key }, // KEYS[1]
            new RedisValue[] { (int)ttl.TotalSeconds } // ARGV[1]
        );
        return (long)result;
    }

    public async Task<long> IncrByWithExpireAsync(string key, long increment, TimeSpan ttl)
    {
        var result = await _cacheDb.ScriptEvaluateAsync(
            IncrByWithExpireScript, // <= string
            new RedisKey[] { (RedisKey)key }, // KEYS[1]
            new RedisValue[] { increment, (int)ttl.TotalSeconds } // ARGV[1], ARGV[2]
        );
        return (long)result;
    }

    public Task<List<T>> GetListDataAsync<T>(string key)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SetListDataAsync<T>(string key, List<T> value, DateTimeOffset? expirationTime)
    {
        throw new NotImplementedException();
    }

    //T ICacheService.GetData<T>(string key)
    //{
    //    var value = _cacheDb.StringGet(key);
    //    if (!string.IsNullOrEmpty(value))
    //    {
    //        return JsonSerializer.Deserialize<T>(value);
    //    }
    //    return default;
    //}

    //List<T> ICacheService.GetListData<T>(string key)
    //{
    //    throw new NotImplementedException();
    //}

    //object ICacheService.RemoveData(string key)
    //{
    //    var exist = _cacheDb.KeyExists(key);
    //    if (exist)
    //    {
    //        return _cacheDb.KeyDelete(key);
    //    }
    //    return false;
    //}

    //bool ICacheService.SetData<T>(string key, T value, DateTimeOffset expirationTime)
    //{
    //    var expirtyTime = expirationTime.DateTime.Subtract(DateTime.Now);
    //    return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expirtyTime);
    //}

    //bool ICacheService.SetListData<T>(string key, List<T> value, DateTimeOffset expirationTime)
    //{
    //    throw new NotImplementedException();
    //}
}