namespace IL.RankedCache.Extensions
{
    internal static class DictionaryExtensions
    {
        public static IEnumerable<KeyValuePair<string, T>> ExcludeReservedEntries<T>(this Dictionary<string, T> dictionary, string[]? reservedPaths) where T : struct
        {
            return reservedPaths != null ? dictionary.Where(x => !reservedPaths.Contains(x.Key)) : dictionary;
        }
    }
}