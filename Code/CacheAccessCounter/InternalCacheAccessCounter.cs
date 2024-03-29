using System.Collections.Concurrent;

namespace IL.RankedCache.CacheAccessCounter
{
    internal class InternalCacheAccessCounter<TCacheCounterOrder> : Dictionary<string, TCacheCounterOrder>, ICacheAccessCounter<TCacheCounterOrder> where TCacheCounterOrder : struct
    {
    }
}