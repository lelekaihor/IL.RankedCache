using IL.RankedCache.CacheProvider;
using IL.RankedCache.Policies;
using IL.RankedCache.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IL.RankedCache.Tests.Services
{
    public class RankedCacheServiceTests
    {
        [Fact]
        public async Task Add_ValidObject_CallsCacheProviderAdd()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var key = "testKey";
            var value = "testValue";

            // Act
            await rankedCacheService.Add(key, value);

            // Assert
            cacheProviderMock.Verify(mock => mock.Add(key, value), Times.Once);
        }

        [Fact]
        public async Task Add_ValidObject_SetsCacheAccessCounter()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var key = "testKey";
            var value = "testValue";

            // Act
            await rankedCacheService.Add(key, value);

            // Assert
            Assert.True(rankedCacheService.HasKey(key));
            Assert.Equal(0, rankedCacheService.GetCacheAccessCounter(key));
        }

        [Fact]
        public async Task Get_CacheEntryByExistingKey_CallsCacheProviderGet_And_IncrementsCacheAccessCounter()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var key = "testKey";
            var value = "testValue";
            await rankedCacheService.Add(key, value);

            // Act
            await rankedCacheService.Get<string>(key);

            // Assert
            cacheProviderMock.Verify(mock => mock.Get<string>(key), Times.Once);
            Assert.Equal(1, rankedCacheService.GetCacheAccessCounter(key));
        }

        [Fact]
        public async Task Delete_CacheEntryByExistingKey_CallsCacheProviderDelete_And_RemovesCacheAccessCounter()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var key = "testKey";
            var value = "testValue";
            await rankedCacheService.Add(key, value);

            // Act
            await rankedCacheService.Delete(key);

            // Assert
            cacheProviderMock.Verify(mock => mock.Delete(key), Times.Once);
            Assert.Null(rankedCacheService.GetCacheAccessCounter(key));
        }
    }
}