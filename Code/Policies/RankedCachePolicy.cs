using IL.RankedCache.Models;

namespace IL.RankedCache.Policies
{
    public class RankedCachePolicy
    {
        public int MaxItems { get; }
        public CleanupMode CleanupMode { get; }
        public TimeSpan? Frequency { get; }

        public RankedCachePolicy(int maxItems, CleanupMode cleanupMode = CleanupMode.Manual, TimeSpan? frequency = null)
        {
            MaxItems = maxItems;
            CleanupMode = cleanupMode;
            if (cleanupMode == CleanupMode.Auto)
            {
                Frequency = frequency ?? throw new ArgumentNullException(nameof(frequency));
            }
        }

        public static RankedCachePolicy Default => new(1000, CleanupMode.Auto, TimeSpan.FromHours(1));
    }
}