using IL.InMemoryCacheProvider.Options;

namespace IL.RankedCache.CacheProvider;

internal sealed class DefaultInMemoryCacheProvider : ICacheProvider
{
    private readonly InMemoryCacheProvider.CacheProvider.ICacheProvider _cacheProvider;

    public DefaultInMemoryCacheProvider(InMemoryCacheProvider.CacheProvider.ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        return await _cacheProvider.GetAsync<T?>(key);
    }

    public T? Get<T>(string key)
    {
        return _cacheProvider.Get<T?>(key);
    }

    public async Task AddAsync<T>(string key, T obj, DateTimeOffset? absoluteExpiration = default)
    {
        await _cacheProvider.AddAsync(key, obj, new ExpirationOptions
        {
            AbsoluteExpiration = absoluteExpiration
        });
    }

    public void Add<T>(string key, T obj, DateTimeOffset? absoluteExpiration = default)
    {
        _cacheProvider.Add(key, obj, new ExpirationOptions
        {
            AbsoluteExpiration = absoluteExpiration
        });
    }

    public async Task DeleteAsync(string key)
    {
        await _cacheProvider.DeleteAsync(key);
    }

    public void Delete(string key)
    {
        _cacheProvider.Delete(key);
    }

    public bool HasKey(string key)
    {
        return _cacheProvider.HasKey(key);
    }
}