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
        /// Default ranked cache service DI initialization
        /// </summary>
        public static void AddRankedCache(this IServiceCollection services, Action<RankedCachePolicy>? options = null)
        {
            services.RegisterCacheProvider<DefaultCacheProvider>();
            services.RegisterDefaultRankedCacheService(options);
        }

        /// <summary>
        /// Default ranked cache service DI initialization with custom implementation of Cache Provider
        /// </summary>
        /// <typeparam name="TCacheProvider">Custom implementation of Cache Provider.</typeparam>
        public static void AddRankedCache<TCacheProvider>(this IServiceCollection services, Action<RankedCachePolicy>? options = null) where TCacheProvider : class, ICacheProvider
        {
            services.RegisterCacheProvider<TCacheProvider>();
            services.RegisterDefaultRankedCacheService(options);
        }

        /// <summary>
        /// Ranked cache service DI initialization
        /// </summary>
        /// <typeparam name="TCacheCounterOrder">Accepts short, int and long as constraints. Will throw NotSupportedException for all other types.</typeparam>
        public static void AddRankedCacheSpecialized<TCacheCounterOrder>(this IServiceCollection services, Action<RankedCachePolicy>? options = null) where TCacheCounterOrder : struct
        {
            if (typeof(TCacheCounterOrder) != typeof(short) && typeof(TCacheCounterOrder) != typeof(int) && typeof(TCacheCounterOrder) != typeof(long))
            {
                throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
            }

            services.RegisterRankedCacheService<TCacheCounterOrder>(options);
            services.RegisterCacheProvider<DefaultCacheProvider>();
        }

        /// <summary>
        /// Ranked cache service DI initialization with custom Cache Provider
        /// </summary>
        /// <typeparam name="TCacheCounterOrder">Accepts short, int and long as constraints. Will throw NotSupportedException for all other types.</typeparam>
        /// <typeparam name="TCacheProvider">Custom implementation of Cache Provider</typeparam>
        public static void AddRankedCacheSpecialized<TCacheCounterOrder, TCacheProvider>(this IServiceCollection services, Action<RankedCachePolicy>? options = null)
            where TCacheCounterOrder : struct
            where TCacheProvider : class, ICacheProvider
        {
            if (typeof(TCacheCounterOrder) != typeof(short) && typeof(TCacheCounterOrder) != typeof(int) && typeof(TCacheCounterOrder) != typeof(long))
            {
                throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
            }

            services.RegisterRankedCacheService<TCacheCounterOrder>(options);
            services.RegisterCacheProvider<TCacheProvider>();
        }

        private static void RegisterDefaultRankedCacheService(this IServiceCollection services, Action<RankedCachePolicy>? options)
        {
            RankedCachePolicy rankedCachePolicy = new();
            options?.Invoke(rankedCachePolicy);
            services.Configure(options ?? (_ => { }));

            services.AddSingleton<IRankedCacheService, RankedCacheService>();
            if (rankedCachePolicy.CachingType == CachingType.SingleInstance)
            {
                services.AddSingleton<ICacheAccessCounter<int>, InternalCacheAccessCounter<int>>();
            }
            else
            {
                services.AddSingleton<ICacheAccessCounter<int>, DistributedCacheAccessCounter<int>>();
            }
        }

        private static void RegisterRankedCacheService<TCacheCounterOrder>(this IServiceCollection services, Action<RankedCachePolicy>? options)
            where TCacheCounterOrder : struct
        {
            RankedCachePolicy rankedCachePolicy = new();
            options?.Invoke(rankedCachePolicy);
            services.Configure(options ?? (_ => { }));

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

        private static void RegisterCacheProvider<TCacheProvider>(this IServiceCollection services) where TCacheProvider : class, ICacheProvider
        {
            services.AddSingleton<ICacheProvider, TCacheProvider>();
        }
    }
}