using IL.RankedCache.Concurrency;
using IL.RankedCache.Services;

namespace IL.RankedCache.Extensions
{
    public static class RankedCacheExtensions
    {
        public static T GetOrAdd<T, TCacheCounterOrder>(this IRankedCacheService<TCacheCounterOrder> rankedCacheService,
            string key,
            Func<T> valueFactory,
            Func<T, bool> cacheCreationCondition,
            DateTimeOffset? absoluteExpiration) where TCacheCounterOrder : struct
        {
            using (LockManager.GetLock(key))
            {
                var value = rankedCacheService.Get<T>(key).Result;
                if (value != null)
                {
                    return value;
                }

                value = valueFactory();
                AddCacheEntryIfJustified(rankedCacheService, key, value, cacheCreationCondition, absoluteExpiration);

                return value;
            }
        }

        public static T GetOrAdd<T, TCacheCounterOrder>(this IRankedCacheService<TCacheCounterOrder> rankedCacheService,
            string key,
            Func<Task<T>> valueFactory,
            Func<T, bool> cacheCreationCondition,
            DateTimeOffset? absoluteExpiration) where TCacheCounterOrder : struct
        {
            using (LockManager.GetLock(key))
            {
                var value = rankedCacheService.Get<T>(key).Result;
                if (value != null)
                {
                    return value;
                }

                value = valueFactory().Result;
                AddCacheEntryIfJustified(rankedCacheService, key, value, cacheCreationCondition, absoluteExpiration);

                return value;
            }
        }

        private static void AddCacheEntryIfJustified<T, TCacheCounterOrder>(IRankedCacheService<TCacheCounterOrder> rankedCacheService, string key,
            T value, Func<T, bool> cacheCreationCondition, DateTimeOffset? absoluteExpiration) where TCacheCounterOrder : struct
        {
            if (cacheCreationCondition(value))
            {
                rankedCacheService.Add(key, value, absoluteExpiration).Wait();
            }
        }

        public static async Task<T> GetOrAddAsync<T, TCacheCounterOrder>(this IRankedCacheService<TCacheCounterOrder> rankedCacheService,
            string key,
            Func<T> valueFactory,
            Func<T, bool> cacheCreationCondition,
            DateTimeOffset? absoluteExpiration) where TCacheCounterOrder : struct
        {
            using (await LockManager.GetLockAsync(key))
            {
                var value = await rankedCacheService.Get<T>(key);
                if (value != null)
                {
                    return value;
                }

                value = valueFactory();
                await AddCacheEntryIfJustifiedAsync(rankedCacheService, key, value, cacheCreationCondition, absoluteExpiration);

                return value;
            }
        }

        public static async Task<T> GetOrAddAsync<T, TCacheCounterOrder>(this IRankedCacheService<TCacheCounterOrder> rankedCacheService,
            string key,
            Func<Task<T>> valueFactory,
            Func<T, bool> cacheCreationCondition,
            DateTimeOffset? absoluteExpiration) where TCacheCounterOrder : struct
        {
            using (await LockManager.GetLockAsync(key))
            {
                var value = await rankedCacheService.Get<T>(key);
                if (value != null)
                {
                    return value;
                }

                value = await valueFactory();
                await AddCacheEntryIfJustifiedAsync(rankedCacheService, key, value, cacheCreationCondition, absoluteExpiration);

                return value;
            }
        }

        private static async Task AddCacheEntryIfJustifiedAsync<T, TCacheCounterOrder>(IRankedCacheService<TCacheCounterOrder> rankedCacheService, string key,
            T value, Func<T, bool> cacheCreationCondition, DateTimeOffset? absoluteExpiration) where TCacheCounterOrder : struct
        {
            if (cacheCreationCondition(value))
            {
                await rankedCacheService.Add(key, value, absoluteExpiration);
            }
        }
    }
}