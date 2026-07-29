namespace Services.Redis;

public interface IRedisService
{
    Task<T?> GetDataAsync<T>(string key);
    Task<List<T>> GetListDataAsync<T>(string key);
    Task<bool> SetDataAsync<T>(string key, T value, DateTimeOffset? expirationTime = null);
    Task<bool> SetListDataAsync<T>(string key, List<T> value, DateTimeOffset? expirationTime = null);
    Task<bool> RemoveDataAsync(string key);
    
    Task<long> IncrWithExpireAsync(string key, TimeSpan ttl);
    
    Task<long> IncrByWithExpireAsync(string key, long increment, TimeSpan ttl);
}