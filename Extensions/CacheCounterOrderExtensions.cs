namespace IL.RankedCache.Extensions
{
    internal static class CacheCounterOrderExtensions
    {
        internal static TCacheCounterOrder Increment<TCacheCounterOrder>(this TCacheCounterOrder value) where TCacheCounterOrder : struct
        {
            if (typeof(TCacheCounterOrder) == typeof(short))
            {
                var shortValue = (short)(object)value;
                if (shortValue < short.MaxValue)
                {
                    shortValue++;
                }

                return (TCacheCounterOrder)(object)shortValue;
            }

            if (typeof(TCacheCounterOrder) == typeof(int))
            {
                var intValue = (int)(object)value;
                if (intValue < int.MaxValue)
                {
                    intValue++;
                }

                return (TCacheCounterOrder)(object)intValue;
            }

            if (typeof(TCacheCounterOrder) == typeof(long))
            {
                var longValue = (long)(object)value;
                if (longValue < long.MaxValue)
                {
                    longValue++;
                }

                return (TCacheCounterOrder)(object)longValue;
            }

            throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
        }
    }
}