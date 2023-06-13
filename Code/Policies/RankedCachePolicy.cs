using IL.RankedCache.Models;

namespace IL.RankedCache.Policies
{
    public class RankedCachePolicy
    {
        /// <summary>
        /// Limits maximum amount of items cache provider can keep after cleanup
        /// </summary>
        public int MaxItems { get; set; } = 1000;

        /// <summary>
        /// Reserved keys that are not going to be affected by cleanup
        /// </summary>
        public string[]? ReservedEntries { get; set; } = null;

        /// <summary>
        /// Option to add custom suffix to all cache entries - might be helpful if reusing same caching provider for different tiers
        /// </summary>
        public string EnvironmentSuffix { get; set; } = string.Empty;

        /// <summary>
        /// Caching type defines where counters are to be stored. Single instance stores counter internally, while Distributed are storing inside caching provider itself.
        /// Distributed processing is role that will perform Cleanups, DistributedSubscriber can only perform caching operations and increase counters.
        /// Default value is SingleInstance.
        /// </summary>
        public CachingType CachingType { get; set; } = CachingType.SingleInstance;

        private CleanupMode _cleanupMode = CleanupMode.Auto;

        /// <summary>
        /// Cleanup mode defines how Cleanup is going to be executed. If frequency is not yet set will be instantiated with default value (1h)
        /// </summary>
        public CleanupMode CleanupMode
        {
            get => _cleanupMode;
            set
            {
                if (value == CleanupMode.Auto && Frequency == null)
                {
                    Frequency = TimeSpan.FromHours(1);
                }

                _cleanupMode = value;
            }
        }

        private TimeSpan? _frequency = TimeSpan.FromHours(1);

        /// <summary>
        /// Frequency of cleanup executions. Must be set if service expected to be used in Auto mode
        /// </summary>
        public TimeSpan? Frequency
        {
            get => _frequency;
            set
            {
                if (value == null && _cleanupMode == CleanupMode.Auto)
                {
                    throw new NotSupportedException("Auto mode requires frequency to be not null!");
                }

                _frequency = value;
            }
        }
    }
}