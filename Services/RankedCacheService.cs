using IL.RankedCache.CacheProvider;
using IL.RankedCache.Models;
using IL.RankedCache.Policy;

namespace IL.RankedCache.Services
{
    public class RankedCacheService : IRankedCacheService, IDisposable
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly RankedCachePolicy _policy = RankedCachePolicy.Default;
        private readonly Dictionary<string, int> _cacheAccessCounter = new();
        private Timer? _cleanupTimer;

        public RankedCacheService(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
            SetupCleanupTimer();
        }

        public RankedCacheService(ICacheProvider cacheProvider, RankedCachePolicy policy) : this(cacheProvider)
        {
            _policy = policy;
        }

        public async Task Add<T>(string key, T obj)
        {
            await _cacheProvider.Add(key, obj);
            _cacheAccessCounter[key] = 0;
        }

        public async Task<T> Get<T>(string key)
        {
            if (_cacheAccessCounter.ContainsKey(key))
            {
                _cacheAccessCounter[key]++;
            }

            return await _cacheProvider.Get<T>(key);
        }

        public async Task Delete(string key)
        {
            await _cacheProvider.Delete(key);
            _cacheAccessCounter.Remove(key);
        }

        public bool HasKey(string key)
        {
            return _cacheAccessCounter.ContainsKey(key);
        }

        public async Task Cleanup()
        {
            var entriesToRemove = _cacheAccessCounter
                .OrderByDescending(kvp => kvp.Value)
                .Skip(_policy.MaxItems)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in entriesToRemove)
            {
                await _cacheProvider.Delete(key);
                _cacheAccessCounter.Remove(key);
            }
        }

        private void SetupCleanupTimer()
        {
            if (_policy.CleanupMode == CleanupMode.Auto)
            {
                _cleanupTimer = new Timer(CleanupCallback!, null, GetInitialDelay(_policy.Frequency!.Value), _policy.Frequency!.Value);
            }
        }

        private static TimeSpan GetInitialDelay(TimeSpan frequency)
        {
            var nextCleanupTime = DateTime.Now.Add(frequency);

            // If the next cleanup time is in the past, calculate the delay until the next occurrence
            if (nextCleanupTime < DateTime.Now)
            {
                var delay = DateTime.Now - nextCleanupTime;
                return frequency + delay;
            }

            return nextCleanupTime - DateTime.Now;
        }

        private void CleanupCallback(object state)
        {
            Cleanup();
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }
}