using IL.RankedCache.Services;

namespace IL.RankedCache.Extensions
{
    public static class RankedCacheExtensions
    {
        public static async Task<T> GetOrAdd<T>(this IRankedCacheService<short> rankedCacheService,
            string key,
            Func<T> valueFactory,
            DateTimeOffset? absoluteExpiration)
        {
            return await GetOrAdd<T, short>(rankedCacheService, key, valueFactory, absoluteExpiration);
        }

        public static async Task<T> GetOrAdd<T>(this IRankedCacheService<int> rankedCacheService,
            string key,
            Func<T> valueFactory,
            DateTimeOffset? absoluteExpiration)
        {
            return await GetOrAdd<T, int>(rankedCacheService, key, valueFactory, absoluteExpiration);
        }

        public static async Task<T> GetOrAdd<T>(this IRankedCacheService<long> rankedCacheService,
            string key,
            Func<T> valueFactory,
            DateTimeOffset? absoluteExpiration)
        {
            return await GetOrAdd<T, long>(rankedCacheService, key, valueFactory, absoluteExpiration);
        }

        private static async Task<T> GetOrAdd<T, TCacheCounterOrder>(this IRankedCacheService<TCacheCounterOrder> rankedCacheService,
            string key,
            Func<T> valueFactory,
            DateTimeOffset? absoluteExpiration) where TCacheCounterOrder : struct
        {
            var value = await rankedCacheService.Get<T>(key);

            if (value is null)
            {
                value = valueFactory();
                await rankedCacheService.Add(key, value, absoluteExpiration);
            }

            return value;
        }
    }
}