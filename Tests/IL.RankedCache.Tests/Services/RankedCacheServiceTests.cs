using IL.RankedCache.CacheProvider;
using IL.RankedCache.Models;
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

        [Fact]
        public async Task HasKey_Returns_Correct_Bool_Value_If_CacheEntryPresent()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var existingKey = "testKey";
            var value = "testValue";
            var nonExistingKey = "nonExistingKey";

            // Act
            await rankedCacheService.Add(existingKey, value);

            // Assert
            Assert.True(rankedCacheService.HasKey(existingKey));
            Assert.False(rankedCacheService.HasKey(nonExistingKey));
        }

        [Fact]
        public async Task Cleanup_RemovesExcessEntries_CallsCacheProviderDelete()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy { MaxItems = 5 });
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var cacheAccessCounter = new Dictionary<string, int>
        {
            { "key1", 5 },
            { "key2", 3 },
            { "key3", 2 },
            { "key4", 7 },
            { "key5", 4 },
            { "key6", 1 },
            { "key7", 6 }
        };
            rankedCacheService.SetCacheAccessCounter(cacheAccessCounter);

            // Act
            await rankedCacheService.Cleanup();

            // Assert
            cacheProviderMock.Verify(mock => mock.Delete(It.IsAny<string>()), Times.Exactly(2));
            cacheProviderMock.Verify(mock => mock.Delete("key3"), Times.Once);
            cacheProviderMock.Verify(mock => mock.Delete("key6"), Times.Once);
        }

        [Fact]
        public async Task Cleanup_ResetsCounters_ResetsCounterForEachEntry()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy { MaxItems = 5 });
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var cacheAccessCounter = new Dictionary<string, int>
        {
            { "key1", 5 },
            { "key2", 3 },
            { "key3", 2 },
            { "key4", 7 },
            { "key5", 4 }
        };
            rankedCacheService.SetCacheAccessCounter(cacheAccessCounter);

            // Act
            await rankedCacheService.Cleanup();

            // Assert
            Assert.Equal(1, rankedCacheService.GetCacheAccessCounter("key1"));
            Assert.Equal(1, rankedCacheService.GetCacheAccessCounter("key2"));
            Assert.Equal(1, rankedCacheService.GetCacheAccessCounter("key3"));
            Assert.Equal(1, rankedCacheService.GetCacheAccessCounter("key4"));
            Assert.Equal(1, rankedCacheService.GetCacheAccessCounter("key5"));
        }

        [Fact]
        public async Task Cleanup_AutomatedMode_RemovesExcessEntries_CallsCacheProviderDelete()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy { MaxItems = 5, CleanupMode = CleanupMode.Auto, Frequency = TimeSpan.FromSeconds(3)});
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, policy);
            var cacheAccessCounter = new Dictionary<string, int>
            {
                { "key1", 5 },
                { "key2", 3 },
                { "key3", 2 },
                { "key4", 7 },
                { "key5", 4 },
                { "key6", 1 },
                { "key7", 6 }
            };
            rankedCacheService.SetCacheAccessCounter(cacheAccessCounter);

            // Act
            Thread.Sleep(TimeSpan.FromSeconds(3));
            //Should perform cleanup automatically
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Assert
            cacheProviderMock.Verify(mock => mock.Delete(It.IsAny<string>()), Times.Exactly(2));
            cacheProviderMock.Verify(mock => mock.Delete("key3"), Times.Once);
            cacheProviderMock.Verify(mock => mock.Delete("key6"), Times.Once);
        }
    }

    internal static class RankedCacheServiceExtensions
    {
        public static async void SetCacheAccessCounter(this RankedCacheService<int> rankedCacheService, Dictionary<string, int> cacheAccessCounter)
        {
            var testObject = "test";
            foreach (var kvp in cacheAccessCounter)
            {
                await rankedCacheService.Add(kvp.Key, testObject);
                for (var i = 0; i < kvp.Value; i++)
                {
                    await rankedCacheService.Get<string>(kvp.Key);
                }
            }
        }
    }
}