using IL.RankedCache.CacheProvider;

namespace IL.RankedCache.Services
{
    public interface IRankedCacheService : ICacheProvider
    {
        Task Cleanup();
    }
}