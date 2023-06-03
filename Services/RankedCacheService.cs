using IL.RankedCache.CacheProvider;
using IL.RankedCache.Models;
using IL.RankedCache.Policy;

namespace IL.RankedCache.Services
{
    /// <summary>
    /// Ranked cache service
    /// </summary>
    /// <typeparam name="TRange">Accepts short, int and long as constraints. Will throw NotSupportedException for all other types.</typeparam>
    public class RankedCacheService<TRange> : IRankedCacheService, IDisposable where TRange : struct
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly RankedCachePolicy _policy = RankedCachePolicy.Default;
        private readonly Dictionary<string, TRange> _cacheAccessCounter = new();
        private Timer? _cleanupTimer;

        /// <summary>
        /// Default constructor will use RankedCachePolicy.Default
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <exception cref="NotSupportedException"></exception>
        public RankedCacheService(ICacheProvider cacheProvider)
        {
            if (typeof(TRange) != typeof(short) || typeof(TRange) != typeof(int) || typeof(TRange) != typeof(long))
            {
                throw new NotSupportedException($"TRange of type {typeof(TRange)} is not supported.");
            }

            _cacheProvider = cacheProvider;
            SetupCleanupTimer();
        }

        /// <summary>
        /// Constructor which allows to specify custom RankedCachePolicy
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="policy"></param>
        public RankedCacheService(ICacheProvider cacheProvider, RankedCachePolicy policy) : this(cacheProvider)
        {
            _policy = policy;
        }

        /// <summary>
        /// Add object to cache, start tracking object access count
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="obj">Object to be cached</param>
        /// <returns>Task</returns>
        public async Task Add<T>(string key, T obj)
        {
            await _cacheProvider.Add(key, obj);
            _cacheAccessCounter[key] = (TRange)(object)0;
        }

        /// <summary>
        /// Get object from cache by key, automatically increases cache access counter for given key
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Object of type specified in constraint</returns>
        public async Task<T> Get<T>(string key)
        {
            if (_cacheAccessCounter.ContainsKey(key))
            {
                _cacheAccessCounter[key] = Increment(_cacheAccessCounter[key]);
            }

            return await _cacheProvider.Get<T>(key);
        }

        /// <summary>
        /// Delete object from cache, automatically deletes corresponding entry in cache access counter
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>Task</returns>
        public async Task Delete(string key)
        {
            await _cacheProvider.Delete(key);
            _cacheAccessCounter.Remove(key);
        }

        /// <summary>
        /// Check if cache has such key
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>True if cache contains such key.</returns>
        public bool HasKey(string key)
        {
            return _cacheAccessCounter.ContainsKey(key);
        }

        /// <summary>
        /// Task to cleanup cache entries according to MaxItems specified in RankedCachePolicy, resets all counters for remaining objects
        /// </summary>
        /// <returns>Task</returns>
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
                _cacheAccessCounter[entryCounter.Key] = (TRange)(object)1;
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

        private static TRange Increment(TRange value)
        {
            if (typeof(TRange) == typeof(short))
            {
                var shortValue = (short)(object)value;
                if (shortValue < short.MaxValue)
                {
                    shortValue++;
                }

                return (TRange)(object)shortValue;
            }

            if (typeof(TRange) == typeof(int))
            {
                var intValue = (int)(object)value;
                if (intValue < int.MaxValue)
                {
                    intValue++;
                }

                return (TRange)(object)intValue;
            }

            if (typeof(TRange) == typeof(long))
            {
                var longValue = (long)(object)value;
                if (longValue < long.MaxValue)
                {
                    longValue++;
                }

                return (TRange)(object)longValue;
            }

            throw new NotSupportedException($"TRange of type {typeof(TRange)} is not supported.");
        }
    }
}