using IL.RankedCache.Extensions;
using IL.RankedCache.Services;
using Moq;
using Xunit;

namespace IL.RankedCache.Tests.Extensions
{
    public class RankedCacheExtensionsTests
    {
        [Fact]
        public async Task GetOrAdd_Returns_ExistingValue_When_CacheContainsKey()
        {
            // Arrange
            var rankedCacheServiceMock = new Mock<IRankedCacheService<int>>();
            var key = "testKey";
            var expectedValue = "newValue";

            rankedCacheServiceMock.Setup(x => x.Get<string>(key)).ReturnsAsync(expectedValue);

            // Act
            var result = await rankedCacheServiceMock.Object.GetOrAdd<string>(
                key,
                () => throw new InvalidOperationException("Value factory should not be called."),
                null);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task GetOrAdd_Returns_NewValue_When_CacheDoesNotContainKey()
        {
            // Arrange
            var rankedCacheServiceMock = new Mock<IRankedCacheService<int>>();
            var key = "testKey";
            var expectedValue = "newValue";
            var valueFactoryCalled = false;

            rankedCacheServiceMock.Setup(x => x.Get<string>(key))
                .ReturnsAsync((string)null);

            // Act
            var result = await rankedCacheServiceMock.Object.GetOrAdd(key,
                () =>
                {
                    valueFactoryCalled = true;
                    return expectedValue;
                },
                null);

            // Assert
            Assert.Equal(expectedValue, result);
            Assert.True(valueFactoryCalled, "Value factory should be called.");
            rankedCacheServiceMock.Verify(x => x.Add(key, expectedValue, null), Times.Once);
        }
    }
}