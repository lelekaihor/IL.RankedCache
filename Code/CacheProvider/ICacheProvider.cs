namespace IL.RankedCache.CacheProvider
{
    public interface ICacheProvider
    {
        void Add<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null);

        Task AddAsync<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);

        void Delete(string key);

        Task DeleteAsync(string key);

        bool HasKey(string key);
    }
}