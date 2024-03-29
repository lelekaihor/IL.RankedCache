namespace IL.RankedCache.CacheProvider;

public interface ICacheProvider
{
    Task<T?> GetAsync<T>(string key);
    T? Get<T>(string key);
    Task AddAsync<T>(string key, T obj, DateTimeOffset? absoluteExpiration = default);
    void Add<T>(string key, T obj, DateTimeOffset? absoluteExpiration = default);
    Task DeleteAsync(string key);
    void Delete(string key);
    bool HasKey(string key);
}