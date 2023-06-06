using System.Runtime.Caching;
using IL.RankedCache.CacheProvider;
using Xunit;

namespace IL.RankedCache.Tests.CacheProvider;

public class DefaultCacheProviderTests
{
    private readonly DefaultCacheProvider _cacheProvider;

    public DefaultCacheProviderTests()
    {
        _cacheProvider = new DefaultCacheProvider();
    }

    [Fact]
    public async Task Add_WithNonNullObject_ShouldAddObjectToCache()
    {
        // Arrange
        var key = "Add_WithNonNullObject_ShouldAddObjectToCache";
        var obj = new TestObject();
        MemoryCache.Default.Trim(100);

        // Act
        await _cacheProvider.Add(key, obj);

        // Assert
        Assert.True(MemoryCache.Default.Contains(key));
    }

    [Fact]
    public async Task Add_WithNullObject_ShouldNotAddObjectToCache()
    {
        // Arrange
        var key = "Add_WithNullObject_ShouldNotAddObjectToCache";
        TestObject? obj = null;
        MemoryCache.Default.Trim(100);

        // Act
        await _cacheProvider.Add(key, obj);

        // Assert
        Assert.False(MemoryCache.Default.Contains(key));
    }

    [Fact]
    public async Task Get_WithExistingKey_ShouldReturnObjectFromCache()
    {
        // Arrange
        var key = "Get_WithExistingKey_ShouldReturnObjectFromCache";
        var obj = new TestObject();
        MemoryCache.Default.Trim(100);

        MemoryCache.Default.Set(key, obj, DateTimeOffset.MaxValue);

        // Act
        var result = await _cacheProvider.Get<TestObject>(key);

        // Assert
        Assert.Equal(obj, result);
    }

    [Fact]
    public async Task Get_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "Get_WithNonExistingKey_ShouldReturnNull";
        MemoryCache.Default.Trim(100);

        // Act
        var result = await _cacheProvider.Get<TestObject>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_WithExistingKey_ShouldRemoveObjectFromCache()
    {
        // Arrange
        var key = "Delete_WithExistingKey_ShouldRemoveObjectFromCache";
        var obj = new TestObject();
        MemoryCache.Default.Trim(100);

        MemoryCache.Default.Set(key, obj, DateTimeOffset.MaxValue);

        // Act
        await _cacheProvider.Delete(key);

        // Assert
        Assert.False(MemoryCache.Default.Contains(key));
    }

    [Fact]
    public void HasKey_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var key = "HasKey_WithExistingKey_ShouldReturnTrue";
        var expectedResult = true;

        var obj = new TestObject();
        MemoryCache.Default.Trim(100);

        MemoryCache.Default.Set(key, obj, DateTimeOffset.MaxValue);

        // Act
        var result = _cacheProvider.HasKey(key);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void HasKey_WithNonExistingKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "HasKey_WithNonExistingKey_ShouldReturnFalse";

        var expectedResult = false;
        var nonExistingKey = "nonExistingKey";

        var obj = new TestObject();
        MemoryCache.Default.Trim(100);

        MemoryCache.Default.Set(key, obj, DateTimeOffset.MaxValue);

        // Act
        var result = _cacheProvider.HasKey(nonExistingKey);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    // Helper class for testing
    private class TestObject
    {
        public string Name { get; set; } = "Test";
        public int Age { get; set; } = 42;
    }
}