using System.Runtime.Caching;

namespace IL.RankedCache.CacheProvider
{
    internal class DefaultCacheProvider : ICacheProvider
    {
        private readonly MemoryCache _cache;
        private readonly object _lockObject;

        public DefaultCacheProvider()
        {
            _cache = MemoryCache.Default;
            _lockObject = new object();
        }

        public Task Add<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null)
        {
            lock (_lockObject)
            {
                if (obj != null)
                {
                    _cache.Set(key, obj, absoluteExpiration ?? DateTimeOffset.MaxValue);
                }
            }

            return Task.CompletedTask;
        }

        public Task<T> Get<T>(string key)
        {
            lock (_lockObject)
            {
                return Task.FromResult((T)_cache.Get(key));
            }
        }

        public Task Delete(string key)
        {
            lock (_lockObject)
            {
                _cache.Remove(key);
            }

            return Task.CompletedTask;
        }

        public bool HasKey(string key)
        {
            return _cache.Contains(key);
        }
    }
}