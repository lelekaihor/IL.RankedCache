using IL.RankedCache.Models;

namespace IL.RankedCache.Policies
{
    public class RankedCachePolicy
    {
        public int MaxItems { get; set; } = 1000;
        public CleanupMode CleanupMode { get; set; } = CleanupMode.Auto;
        public TimeSpan? Frequency { get; set; } = TimeSpan.FromHours(1);
    }
}