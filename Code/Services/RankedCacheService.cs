using IL.RankedCache.CacheAccessCounter;
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
    internal class RankedCacheService<TCacheCounterOrder> : IRankedCacheService<TCacheCounterOrder>
        where TCacheCounterOrder : struct
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly RankedCachePolicy _policy;
        private readonly ICacheAccessCounter<TCacheCounterOrder> _cacheAccessCounter;
        private Timer? _cleanupTimer;
        private Timer? _syncTimer;

        /// <summary>
        /// Default constructor will use RankedCachePolicy.Default
        /// </summary>
        /// <param name="cacheProvider">Cache provider of your choice.</param>
        /// <param name="cacheAccessCounter">Cache access counter</param>
        /// <param name="policy">Ranked cache policy</param>
        /// <exception cref="NotSupportedException"></exception>
        public RankedCacheService(ICacheProvider cacheProvider,
            ICacheAccessCounter<TCacheCounterOrder> cacheAccessCounter, IOptions<RankedCachePolicy> policy)
        {
            if (typeof(TCacheCounterOrder) != typeof(short) && typeof(TCacheCounterOrder) != typeof(int) &&
                typeof(TCacheCounterOrder) != typeof(long))
            {
                throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
            }

            _cacheProvider = cacheProvider;
            _cacheAccessCounter = cacheAccessCounter;
            _policy = policy.Value;
            SetupCleanupTimer();
            SetupCounterEntriesInvalidationForExpiredCacheEntries();
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.Add{T}" />
        public void Add<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = default)
        {
            if (obj != null)
            {
                _cacheProvider.Add(KeyWithSuffix(key), obj, absoluteExpiration);
                _cacheAccessCounter[KeyWithSuffix(key)] = (TCacheCounterOrder)(object)0;
            }
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.AddAsync{T}" />
        public async Task AddAsync<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = default)
        {
            if (obj != null)
            {
                await _cacheProvider.AddAsync(KeyWithSuffix(key), obj, absoluteExpiration);
                _cacheAccessCounter[KeyWithSuffix(key)] = (TCacheCounterOrder)(object)0;
            }
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.Get{T}" />
        public T Get<T>(string key)
        {
            if (HasKey(key))
            {
                _cacheAccessCounter[key] = _cacheAccessCounter[KeyWithSuffix(key)].Increment();
            }

            var result = _cacheProvider.Get<T>(KeyWithSuffix(key));
            if (result == null)
            {
                _cacheAccessCounter.Remove(KeyWithSuffix(key));
            }

            return result!;
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.GetAsync{T}" />
        public async Task<T> GetAsync<T>(string key)
        {
            if (HasKey(key))
            {
                _cacheAccessCounter[key] = _cacheAccessCounter[KeyWithSuffix(key)].Increment();
            }

            var result = await _cacheProvider.GetAsync<T>(KeyWithSuffix(key));
            if (result == null)
            {
                _cacheAccessCounter.Remove(KeyWithSuffix(key));
            }

            return result!;
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.Delete" />
        public void Delete(string key)
        {
            _cacheProvider.Delete(KeyWithSuffix(key));
            _cacheAccessCounter.Remove(KeyWithSuffix(key));
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.DeleteAsync" />
        public async Task DeleteAsync(string key)
        {
            await _cacheProvider.DeleteAsync(KeyWithSuffix(key));
            _cacheAccessCounter.Remove(KeyWithSuffix(key));
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.HasKey" />
        public bool HasKey(string key)
        {
            return _cacheAccessCounter.ContainsKey(KeyWithSuffix(key));
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.Cleanup" />
        public async Task Cleanup()
        {
            if (_policy.CachingType == CachingType.DistributedSubscriber)
            {
                return;
            }

            if (_cacheAccessCounter.Count > _policy.MaxItems)
            {
                var entriesToRemove = _cacheAccessCounter
                    .ExcludeReservedEntries(_policy.ReservedEntries)
                    .OrderByDescending(kvp => kvp.Value)
                    .Skip(_policy.MaxItems)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in entriesToRemove)
                {
                    await _cacheProvider.DeleteAsync(key);
                    _cacheAccessCounter.Remove(key);
                }
            }

            //Reset counters on each cleanup - supposed to allow new cache entries to take over old top ranked in previous iteration
            foreach (var entryCounter in _cacheAccessCounter)
            {
                _cacheAccessCounter[entryCounter.Key] = (TCacheCounterOrder)(object)1;
            }
        }

        public void CounterEntriesInvalidationForExpiredCacheEntries()
        {
            foreach (var key in _cacheAccessCounter.Keys)
            {
                if (!_cacheProvider.HasKey(key))
                {
                    _cacheAccessCounter.Remove(key);
                }
            }
        }

        /// <inheritdoc cref="IRankedCacheService{TCacheCounterOrder}.GetCacheAccessCounter" />
        public TCacheCounterOrder? GetCacheAccessCounter(string key)
        {
            return HasKey(key) ? _cacheAccessCounter[KeyWithSuffix(key)] : null;
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _syncTimer?.Dispose();
        }

        private void SetupCleanupTimer()
        {
            if (_policy.CleanupMode == CleanupMode.Auto && _policy.CachingType == CachingType.SingleInstance ||
                _policy.CachingType == CachingType.DistributedProcessing)
            {
                _cleanupTimer = new Timer(CleanupCallback!, null, GetInitialDelay(_policy.Frequency!.Value),
                    _policy.Frequency!.Value);
            }
        }

        private void SetupCounterEntriesInvalidationForExpiredCacheEntries()
        {
            if (_policy.OutdatedCounterEntriesSyncSettings != null &&
                _policy.OutdatedCounterEntriesSyncSettings!.OutdatedCounterEntriesSyncEnabled)
            {
                _syncTimer = new Timer(CounterEntriesInvalidationForExpiredCacheEntriesCallback!, null,
                    GetInitialDelay(_policy.OutdatedCounterEntriesSyncSettings.Frequency),
                    _policy.OutdatedCounterEntriesSyncSettings.Frequency);
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

        private void CounterEntriesInvalidationForExpiredCacheEntriesCallback(object state)
        {
            CounterEntriesInvalidationForExpiredCacheEntries();
        }

        private string KeyWithSuffix(string key)
        {
            return key + _policy.EnvironmentSuffix;
        }
    }
}