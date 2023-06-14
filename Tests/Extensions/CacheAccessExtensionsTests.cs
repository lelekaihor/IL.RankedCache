using IL.RankedCache.CacheAccessCounter;
using IL.RankedCache.Extensions;
using Xunit;

namespace IL.RankedCache.Tests.Extensions;

public class CacheAccessExtensionsTests
{
    [Fact]
    public void ExcludeReservedEntries_Returns_AllEntries_When_ReservedPaths_IsNull()
    {
        // Arrange
        ICacheAccessCounter<int> accessCounter = new InternalCacheAccessCounter<int>
        {
            {"key1", 1},
            {"key2", 2},
            {"key3", 3}
        };
        var expectedEntries = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 2),
            new KeyValuePair<string, int>("key3", 3)
        };

        // Act
        var result = accessCounter.ExcludeReservedEntries(null);

        // Assert
        Assert.Equal(expectedEntries, result);
    }

    [Fact]
    public void ExcludeReservedEntries_Returns_Entries_Without_ReservedPaths()
    {
        // Arrange
        ICacheAccessCounter<int> accessCounter = new InternalCacheAccessCounter<int>
        {
            {"key1", 1},
            {"key2", 2},
            {"key3", 3},
            {"key4", 4}
        };
        var reservedPaths = new[] { "key2", "key4" };
        var expectedEntries = new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key3", 3)
        };

        // Act
        var result = accessCounter.ExcludeReservedEntries(reservedPaths);

        // Assert
        Assert.Equal(expectedEntries, result);
    }
}