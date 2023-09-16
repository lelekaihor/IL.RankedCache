using IL.InMemoryCacheProvider.CacheProvider;
using IL.RankedCache.CacheAccessCounter;
using Moq;
using Xunit;

namespace IL.RankedCache.Tests.CacheAccessCounter;

public class DistributedCacheAccessCounterTests
{
    [Fact]
    public void Add_IncreasesCountAndAddsToCacheProvider()
    {
        // Arrange
        var cacheProviderMock = new Mock<ICacheProvider>();
        var counter = new DistributedCacheAccessCounter<int>(cacheProviderMock.Object);
        var key = "key";
        var value = 42;

        // Act
        counter.Add(key, value);

        // Assert
        Assert.Equal(1, counter.Count);
        cacheProviderMock.Verify(x => x.AddAsync(key + "_count", value, null), Times.Once);
    }

    [Fact]
    public void Remove_DecreasesCountAndRemovesFromCacheProvider()
    {
        // Arrange
        var cacheProviderMock = new Mock<ICacheProvider>();
        var counter = new DistributedCacheAccessCounter<int>(cacheProviderMock.Object);
        var key = "key";
        var value = 42;
        counter.Add(key, value);

        // Act
        var result = counter.Remove(key);

        // Assert
        Assert.True(result);
        Assert.Equal(0, counter.Count);
        cacheProviderMock.Verify(x => x.Delete(key + "_count"), Times.Once);
    }

    [Fact]
    public void Clear_RemovesAllItemsFromCounterAndCacheProvider()
    {
        // Arrange
        var cacheProviderMock = new Mock<ICacheProvider>();
        var counter = new DistributedCacheAccessCounter<int>(cacheProviderMock.Object)
        {
            { "key1", 1 },
            { "key2", 2 },
            { "key3", 3 }
        };

        // Act
        counter.Clear();

        // Assert
        Assert.Equal(0, counter.Count);
        cacheProviderMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Exactly(3));
    }

    [Fact]
    public void ContainsKey_ReturnsTrueIfKeyExists()
    {
        // Arrange
        var cacheProviderMock = new Mock<ICacheProvider>();
        var counter = new DistributedCacheAccessCounter<int>(cacheProviderMock.Object);
        var key = "key";
        counter.Add(key, 42);

        // Act
        var result = counter.ContainsKey(key);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsKey_ReturnsFalseIfKeyDoesNotExist()
    {
        // Arrange
        var cacheProviderMock = new Mock<ICacheProvider>();
        var counter = new DistributedCacheAccessCounter<int>(cacheProviderMock.Object);
        var key = "key";

        // Act
        var result = counter.ContainsKey(key);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryGetValue_ReturnsTrueAndValueIfKeyExists()
    {
        // Arrange
        var cacheProviderMock = new Mock<ICacheProvider>();
        var counter = new DistributedCacheAccessCounter<int>(cacheProviderMock.Object);
        var key = "key";
        var value = 42;
        counter.Add(key, value);
        cacheProviderMock.Setup(x => x.GetAsync<int>(key + "_count")).ReturnsAsync(value);

        // Act
        var result = counter.TryGetValue(key, out var retrievedValue);

        // Assert
        Assert.True(result);
        Assert.Equal(value, retrievedValue);
    }

    [Fact]
    public void TryGetValue_ReturnsFalseAndDefaultIfKeyDoesNotExist()
    {
        // Arrange
        var cacheProviderMock = new Mock<ICacheProvider>();
        var counter = new DistributedCacheAccessCounter<int>(cacheProviderMock.Object);
        var key = "key";
        var value = 42;

        // Act
        var result = counter.TryGetValue(key, out var retrievedValue);

        // Assert
        Assert.False(result);
        Assert.Equal(default(int), retrievedValue);
    }
}