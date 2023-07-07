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

        // Act
        await _cacheProvider.AddAsync(key, obj);
        // Assert
        Assert.True(MemoryCache.Default.Contains(key));
    }

    [Fact]
    public async Task Add_WithNullObject_ShouldNotAddObjectToCache()
    {
        // Arrange
        var key = "Add_WithNullObject_ShouldNotAddObjectToCache";
        TestObject? obj = null;

        // Act
        await _cacheProvider.AddAsync(key, obj);

        // Assert
        Assert.False(MemoryCache.Default.Contains(key));
    }

    [Fact]
    public async Task Get_WithExistingKey_ShouldReturnObjectFromCache()
    {
        // Arrange
        var key = "Get_WithExistingKey_ShouldReturnObjectFromCache";
        var obj = new TestObject();

        MemoryCache.Default.Set(key, obj, DateTimeOffset.MaxValue);

        // Act
        var result = await _cacheProvider.GetAsync<TestObject>(key);

        // Assert
        Assert.Equal(obj, result);
    }

    [Fact]
    public async Task Get_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "Get_WithNonExistingKey_ShouldReturnNull";

        // Act
        var result = await _cacheProvider.GetAsync<TestObject>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_WithExistingKey_ShouldRemoveObjectFromCache()
    {
        // Arrange
        var key = "Delete_WithExistingKey_ShouldRemoveObjectFromCache";
        var obj = new TestObject();

        MemoryCache.Default.Set(key, obj, DateTimeOffset.MaxValue);

        // Act
        await _cacheProvider.DeleteAsync(key);

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

        MemoryCache.Default.Set(key, obj, DateTimeOffset.MaxValue);

        // Act
        var result = _cacheProvider.HasKey(nonExistingKey);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Add_WithExpiration_WithNonNullObject_ShouldDeleteObject_When_Expired()
    {
        // Arrange
        var key = "    public async Task Add_WithExpiration_WithNonNullObject_ShouldDeleteObject_When_Expired()\r\n";
        var obj = new TestObject();

        // Act
        await _cacheProvider.AddAsync(key, obj, DateTimeOffset.Now.AddSeconds(3));
        // Assert
        Assert.True(MemoryCache.Default.Contains(key));
        Thread.Sleep(4000);
        Assert.False(MemoryCache.Default.Contains(key));
    }

    // Helper class for testing
    private class TestObject
    {
        public string Name { get; set; } = "Test";
        public int Age { get; set; } = 42;
    }
}