using IL.RankedCache.CacheAccessCounter;

namespace IL.RankedCache.Extensions
{
    internal static class CacheAccessCounterExtensions
    {
        public static IEnumerable<KeyValuePair<string, TCacheCounterOrder>> ExcludeReservedEntries<TCacheCounterOrder>(this ICacheAccessCounter<TCacheCounterOrder> cacheAccessCounter, string[]? reservedPaths) where TCacheCounterOrder : struct
        {
            return reservedPaths != null ? cacheAccessCounter.Where(x => !reservedPaths.Contains(x.Key)) : cacheAccessCounter;
        }
    }
}