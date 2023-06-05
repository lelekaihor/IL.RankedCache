using IL.RankedCache.CacheProvider;
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

            services.Configure(options ?? (_ => { }));
            services.AddSingleton<ICacheProvider, DefaultCacheProvider>();
            services.AddSingleton<IRankedCacheService<TCacheCounterOrder>, RankedCacheService<TCacheCounterOrder>>();
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

            services.Configure(options ?? (_ => { }));
            services.AddSingleton<ICacheProvider, TCacheProvider>();
            services.AddSingleton<IRankedCacheService<TCacheCounterOrder>, RankedCacheService<TCacheCounterOrder>>();
        }
    }
}