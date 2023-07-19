using IL.RankedCache.CacheProvider;

namespace IL.RankedCache.Services;

/// <summary>
/// Ranked cache interface
/// </summary>
public interface IRankedCacheService<TCacheCounterOrder> : ICacheProvider, IDisposable where TCacheCounterOrder : struct
{
    /// <summary>
    /// Add object to cache, start tracking object access count
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="obj">Object to be cached</param>
    /// <param name="absoluteExpiration">Optional expiration parameter</param>
    /// <returns>Task</returns>
    new void Add<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null);

    /// <summary>
    /// (async)Add object to cache, start tracking object access count
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="obj">Object to be cached</param>
    /// <param name="absoluteExpiration">Optional expiration parameter</param>
    /// <returns>Task</returns>
    new Task AddAsync<T>(string key, T? obj, DateTimeOffset? absoluteExpiration = null);

    /// <summary>
    /// Get object from cache by key, automatically increases cache access counter for given key
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Object of type specified in constraint</returns>
    new T Get<T>(string key);

    /// <summary>
    /// (async)Get object from cache by key, automatically increases cache access counter for given key
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Object of type specified in constraint</returns>
    new Task<T> GetAsync<T>(string key);

    /// <summary>
    /// Delete object from cache, automatically deletes corresponding entry in cache access counter
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Task</returns>
    new void Delete(string key);

    /// <summary>
    /// (async)Delete object from cache, automatically deletes corresponding entry in cache access counter
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Task</returns>
    new Task DeleteAsync(string key);

    /// <summary>
    /// Check if cache has such key
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if cache contains such key.</returns>
    new bool HasKey(string key);

    /// <summary>
    /// (async)Task to cleanup cache entries according to MaxItems specified in RankedCachePolicy, resets all counters for remaining objects
    /// </summary>
    /// <returns>Task</returns>
    Task Cleanup();

    /// <summary>
    /// Get the counter of how many times cache entry was accessed
    /// </summary>
    TCacheCounterOrder? GetCacheAccessCounter(string key);
}