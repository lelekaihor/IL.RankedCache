using IL.RankedCache.CacheAccessCounter;
using IL.RankedCache.CacheProvider;
using IL.RankedCache.Models;
using IL.RankedCache.Policies;
using IL.RankedCache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IL.RankedCache.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Ranked cache service DI initialization
        /// </summary>
        /// <typeparam name="TCacheCounterOrder">Accepts short, int and long as constraints. Will throw NotSupportedException for all other types.</typeparam>
        public static void AddRankedCache<TCacheCounterOrder>(this IServiceCollection services, Action<RankedCachePolicy>? options = null) where TCacheCounterOrder : struct
        {
            if (typeof(TCacheCounterOrder) != typeof(short) && typeof(TCacheCounterOrder) != typeof(int) && typeof(TCacheCounterOrder) != typeof(long))
            {
                throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
            }

            RegisterDependencies<TCacheCounterOrder, DefaultCacheProvider>(services, options);
        }

        /// <summary>
        /// Ranked cache service DI initialization with custom Cache Provider
        /// </summary>
        /// <typeparam name="TCacheCounterOrder">Accepts short, int and long as constraints. Will throw NotSupportedException for all other types.</typeparam>
        /// <typeparam name="TCacheProvider">Custom implementation of Cache Provider</typeparam>
        public static void AddRankedCache<TCacheCounterOrder, TCacheProvider>(this IServiceCollection services, Action<RankedCachePolicy>? options = null)
            where TCacheCounterOrder : struct
            where TCacheProvider : class, ICacheProvider
        {
            if (typeof(TCacheCounterOrder) != typeof(short) && typeof(TCacheCounterOrder) != typeof(int) && typeof(TCacheCounterOrder) != typeof(long))
            {
                throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
            }

            RegisterDependencies<TCacheCounterOrder, TCacheProvider>(services, options);
        }

        private static void RegisterDependencies<TCacheCounterOrder, TCacheProvider>(IServiceCollection services, Action<RankedCachePolicy>? options)
            where TCacheCounterOrder : struct
            where TCacheProvider : class, ICacheProvider
        {
            RankedCachePolicy rankedCachePolicy = new();
            options?.Invoke(rankedCachePolicy);
            services.Configure(options ?? (_ => { }));

            services.AddSingleton<ICacheProvider, TCacheProvider>();
            services.AddSingleton<IRankedCacheService<TCacheCounterOrder>, RankedCacheService<TCacheCounterOrder>>();
            if (rankedCachePolicy.CachingType == CachingType.SingleInstance)
            {
                services.AddSingleton<ICacheAccessCounter<TCacheCounterOrder>, InternalCacheAccessCounter<TCacheCounterOrder>>();
            }
            else
            {
                services.AddSingleton<ICacheAccessCounter<TCacheCounterOrder>, DistributedCacheAccessCounter<TCacheCounterOrder>>();
            }
        }
    }
}