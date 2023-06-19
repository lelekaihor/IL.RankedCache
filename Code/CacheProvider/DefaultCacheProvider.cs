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

        public Task Add<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null)
        {
            if (obj != null)
            {
                _cache.Set(key, obj, absoluteExpiration ?? DateTimeOffset.MaxValue);
            }

            return Task.CompletedTask;
        }

        public Task<T> Get<T>(string key)
        {
            return Task.FromResult((T)_cache.Get(key));
        }

        public Task Delete(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public bool HasKey(string key)
        {
            return _cache.Contains(key);
        }
    }
}