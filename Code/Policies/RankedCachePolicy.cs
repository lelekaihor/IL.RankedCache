using IL.RankedCache.Models;

namespace IL.RankedCache.Policies
{
    public class RankedCachePolicy
    {
        public int MaxItems { get; set; } = 1000;

        private CleanupMode _cleanupMode = CleanupMode.Auto;

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