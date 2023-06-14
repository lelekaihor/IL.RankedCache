namespace IL.RankedCache.Extensions
{
    internal static class CacheCounterOrderExtensions
    {
        internal static TCacheCounterOrder Increment<TCacheCounterOrder>(this TCacheCounterOrder value) where TCacheCounterOrder : struct
        {
            switch (Type.GetTypeCode(typeof(TCacheCounterOrder)))
            {
                case TypeCode.Int16:
                    var shortValue = (short)(object)value;
                    if (shortValue < short.MaxValue)
                    {
                        shortValue++;
                    }

                    return (TCacheCounterOrder)(object)shortValue;

                case TypeCode.Int32:
                    var intValue = (int)(object)value;
                    if (intValue < int.MaxValue)
                    {
                        intValue++;
                    }

                    return (TCacheCounterOrder)(object)intValue;

                case TypeCode.Int64:
                    var longValue = (long)(object)value;
                    if (longValue < long.MaxValue)
                    {
                        longValue++;
                    }

                    return (TCacheCounterOrder)(object)longValue;

                default:
                    throw new NotSupportedException($"TRange of type {typeof(TCacheCounterOrder)} is not supported.");
            }
        }
    }
}