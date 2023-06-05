using IL.RankedCache.CacheProvider;
using IL.RankedCache.Extensions;
using IL.RankedCache.Models;
using IL.RankedCache.Policies;
using Microsoft.Extensions.Options;

namespace IL.RankedCache.Services
{
    /// <summary>
    /// Ranked cache service
    /// </summary>
    /// <typeparam name="TCacheCounterOrder">Accepts short, int and long as constraints. Will throw NotSupportedException for all other types.</typeparam>
    internal class RankedCacheService<TCacheCounterOrder> : IRankedCacheService where TCacheCounterOrder : struct
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly RankedCachePolicy _policy;
        private readonly Dictionary<string, TCacheCounterOrder> _cacheAccessCounter = new();
        private Timer? _cleanupTimer;

        /// <summary>
        /// Default constructor will use RankedCachePolicy.Default
        /// </summary>
        /// <param name="cacheProvider">Cache provider of your choice.</param>
        /// <param name="policy">Ranked cache policy</param>
        /// <exception cref="NotSupportedException"></exception>
        internal RankedCacheService(ICacheProvider cacheProvider, IOptions<RankedCachePolicy>? policy)
        {
            if (typeof(TCacheCounterOrder) != typeof(short) || typeof(TCacheCounterOrder) != typeof(int) || typeof(TCacheCounterOrder) != typeof(long))
            {
                throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
            }

            _cacheProvider = cacheProvider;
            _policy = policy?.Value ?? RankedCachePolicy.Default;
            SetupCleanupTimer();
        }

        /// <inheritdoc cref="IRankedCacheService.Add" />
        public async Task Add<T>(string key, T? obj)
        {
            if (obj != null)
            {
                await _cacheProvider.Add(key, obj);
                _cacheAccessCounter[key] = (TCacheCounterOrder)(object)0;
            }
        }

        /// <inheritdoc cref="IRankedCacheService.Get" />
        public async Task<T> Get<T>(string key)
        {
            if (_cacheAccessCounter.ContainsKey(key))
            {
                _cacheAccessCounter[key] = _cacheAccessCounter[key].Increment();
            }

            return await _cacheProvider.Get<T>(key);
        }

        /// <inheritdoc cref="IRankedCacheService.Delete" />
        public async Task Delete(string key)
        {
            await _cacheProvider.Delete(key);
            _cacheAccessCounter.Remove(key);
        }

        /// <inheritdoc cref="IRankedCacheService.HasKey" />
        public bool HasKey(string key)
        {
            return _cacheAccessCounter.ContainsKey(key);
        }

        /// <inheritdoc cref="IRankedCacheService.Cleanup" />
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

            //Reset counters on each cleanup - supposed to allow new cache entries to take over old top ranked in previous iteration
            foreach (var entryCounter in _cacheAccessCounter)
            {
                _cacheAccessCounter[entryCounter.Key] = (TCacheCounterOrder)(object)1;
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
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
            _ = Cleanup();
        }
    }
}