using System.Runtime.Caching;

namespace IL.RankedCache.CacheProvider
{
    internal class DefaultCacheProvider : ICacheProvider
    {
        private readonly MemoryCache _cache;

        public DefaultCacheProvider()
        {
            _cache = MemoryCache.Default;
        }

        public void Add<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null)
        {
            if (obj != null)
            {
                _cache.Set(key, obj, absoluteExpiration ?? DateTimeOffset.MaxValue);
            }
        }

        public Task AddAsync<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null)
        {
            Add(key, obj, absoluteExpiration);
            return Task.CompletedTask;
        }

        public T Get<T>(string key) => (T)_cache.Get(key);

        public Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(Get<T>(key));
        }

        public void Delete(string key)
        {
            _cache.Remove(key);
        }

        public Task DeleteAsync(string key)
        {
            Delete(key);
            return Task.CompletedTask;
        }

        public bool HasKey(string key)
        {
            return _cache.Contains(key);
        }
    }
}