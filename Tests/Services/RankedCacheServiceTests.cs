using IL.InMemoryCacheProvider.CacheProvider;
using IL.RankedCache.CacheAccessCounter;
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
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var key = "testKey";
            var value = "testValue";

            // Act
            await rankedCacheService.AddAsync(key, value);

            // Assert
            cacheProviderMock.Verify(mock => mock.AddAsync(key, value, null), Times.Once);
        }

        [Fact]
        public async Task Add_ValidObject_SetsCacheAccessCounter()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var key = "testKey";
            var value = "testValue";

            // Act
            await rankedCacheService.AddAsync(key, value);

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
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var key = "testKey";
            var value = "testValue";
            cacheProviderMock.Setup(x => x.GetAsync<string>(key)).ReturnsAsync(value);
            await rankedCacheService.AddAsync(key, value);

            // Act
            await rankedCacheService.GetAsync<string>(key);

            // Assert
            cacheProviderMock.Verify(mock => mock.GetAsync<string>(key), Times.Once);
            Assert.Equal(1, rankedCacheService.GetCacheAccessCounter(key));
        }

        [Fact]
        public async Task Get_CacheEntryByExistingKey_Removes_Tracking_For_Expired_Cache_Entries()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var key = "testKey";
            var value = "testValue";

            // Act 1
            await rankedCacheService.AddAsync(key, value);

            // Assert 1
            Assert.Equal(0, rankedCacheService.GetCacheAccessCounter(key));
            Assert.True(rankedCacheService.HasKey(key));

            // Act 2
            await rankedCacheService.GetAsync<string>(key);

            // Assert 2
            cacheProviderMock.Verify(mock => mock.GetAsync<string>(key), Times.Once);
            Assert.False(rankedCacheService.HasKey(key));
            Assert.Null(rankedCacheService.GetCacheAccessCounter(key));
        }

        [Fact]
        public async Task Delete_CacheEntryByExistingKey_CallsCacheProviderDelete_And_RemovesCacheAccessCounter()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var key = "testKey";
            var value = "testValue";
            await rankedCacheService.AddAsync(key, value);

            // Act
            await rankedCacheService.DeleteAsync(key);

            // Assert
            cacheProviderMock.Verify(mock => mock.DeleteAsync(key), Times.Once);
            Assert.Null(rankedCacheService.GetCacheAccessCounter(key));
        }

        [Fact]
        public async Task HasKey_Returns_Correct_Bool_Value_If_CacheEntryPresent()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy());
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var existingKey = "testKey";
            var value = "testValue";
            var nonExistingKey = "nonExistingKey";

            // Act
            await rankedCacheService.AddAsync(existingKey, value);

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
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var testObject = "test";
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
            rankedCacheService.SetCacheAccessCounter(cacheAccessCounter, testObject, cacheProviderMock);

            // Act
            await rankedCacheService.Cleanup();

            // Assert
            cacheProviderMock.Verify(mock => mock.DeleteAsync(It.IsAny<string>()), Times.Exactly(2));
            cacheProviderMock.Verify(mock => mock.DeleteAsync("key3"), Times.Once);
            cacheProviderMock.Verify(mock => mock.DeleteAsync("key6"), Times.Once);
        }

        [Fact]
        public async Task Cleanup_ResetsCounters_ResetsCounterForEachEntry()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy { MaxItems = 5 });
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var testObject = "test";
            var cacheAccessCounter = new Dictionary<string, int>
        {
            { "key1", 5 },
            { "key2", 3 },
            { "key3", 2 },
            { "key4", 7 },
            { "key5", 4 }
        };
            rankedCacheService.SetCacheAccessCounter(cacheAccessCounter, testObject, cacheProviderMock);

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
        public void Cleanup_AutomatedMode_RemovesExcessEntries_CallsCacheProviderDelete()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy { MaxItems = 5, CleanupMode = CleanupMode.Auto, Frequency = TimeSpan.FromSeconds(3) });
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var testObject = "test";
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
            rankedCacheService.SetCacheAccessCounter(cacheAccessCounter, testObject, cacheProviderMock);

            // Act
            Thread.Sleep(TimeSpan.FromSeconds(3));
            //Should perform cleanup automatically
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Assert
            cacheProviderMock.Verify(mock => mock.DeleteAsync(It.IsAny<string>()), Times.Exactly(2));
            cacheProviderMock.Verify(mock => mock.DeleteAsync("key3"), Times.Once);
            cacheProviderMock.Verify(mock => mock.DeleteAsync("key6"), Times.Once);
        }

        [Fact]
        public void Cleanup_AutomatedMode_RemovesExcessEntries_ExcludingReserved_CallsCacheProviderDelete()
        {
            // Arrange
            var cacheProviderMock = new Mock<ICacheProvider>();
            var policy = Options.Create(new RankedCachePolicy
            {
                MaxItems = 5,
                CleanupMode = CleanupMode.Auto,
                Frequency = TimeSpan.FromSeconds(3),
                ReservedEntries = new[] { "key3" }
            });
            var rankedCacheService = new RankedCacheService<int>(cacheProviderMock.Object, new InternalCacheAccessCounter<int>(), policy);
            var testObject = "test";
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
            rankedCacheService.SetCacheAccessCounter(cacheAccessCounter, testObject, cacheProviderMock);

            // Act
            Thread.Sleep(TimeSpan.FromSeconds(3));
            //Should perform cleanup automatically
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Assert
            cacheProviderMock.Verify(mock => mock.DeleteAsync(It.IsAny<string>()), Times.Exactly(1));
            cacheProviderMock.Verify(mock => mock.DeleteAsync("key6"), Times.Once);
        }
    }

    internal static class RankedCacheServiceExtensions
    {
        public static async void SetCacheAccessCounter(this RankedCacheService<int> rankedCacheService,
            Dictionary<string, int> cacheAccessCounter, string testObject, Mock<ICacheProvider> cacheProviderMock)
        {
            foreach (var kvp in cacheAccessCounter)
            {
                await rankedCacheService.AddAsync(kvp.Key, testObject);
                cacheProviderMock.Setup(x => x.GetAsync<string>(kvp.Key)).ReturnsAsync(testObject);
                for (var i = 0; i < kvp.Value; i++)
                {
                    await rankedCacheService.GetAsync<string>(kvp.Key);
                }
            }
        }
    }
}