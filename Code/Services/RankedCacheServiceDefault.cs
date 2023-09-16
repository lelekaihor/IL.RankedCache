using IL.InMemoryCacheProvider.CacheProvider;
using IL.RankedCache.CacheAccessCounter;
using IL.RankedCache.Policies;
using Microsoft.Extensions.Options;

namespace IL.RankedCache.Services
{
    internal class RankedCacheService : RankedCacheService<int>, IRankedCacheService
    {
        public RankedCacheService(ICacheProvider cacheProvider, ICacheAccessCounter<int> cacheAccessCounter, IOptions<RankedCachePolicy> policy) : base(cacheProvider, cacheAccessCounter, policy)
        {
        }
    }
}