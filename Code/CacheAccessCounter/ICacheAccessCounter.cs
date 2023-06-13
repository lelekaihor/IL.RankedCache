namespace IL.RankedCache.CacheAccessCounter
{
    internal interface ICacheAccessCounter<TCacheCounterOrder> : IDictionary<string, TCacheCounterOrder> where TCacheCounterOrder : struct
    {
    }
}