using IL.RankedCache.Extensions;
using Xunit;

namespace IL.RankedCache.Tests.Extensions;

public class CacheCounterOrderExtensionsTests
{
    [Fact]
    public void Increment_Short_ReturnsIncrementedValue()
    {
        // Arrange
        short value = 42;

        // Act
        var result = value.Increment();

        // Assert
        Assert.Equal(43, result);
    }

    [Fact]
    public void Increment_Short_MaxValue_Returns_Max_Value()
    {
        // Arrange
        short value = short.MaxValue;

        // Act
        var result = value.Increment();

        // Assert
        Assert.Equal(short.MaxValue, result);
    }

    [Fact]
    public void Increment_Int_ReturnsIncrementedValue()
    {
        // Arrange
        int value = 42;

        // Act
        var result = value.Increment();

        // Assert
        Assert.Equal(43, result);
    }

    [Fact]
    public void Increment_Int_MaxValue_ThrowsNotSupportedException()
    {
        // Arrange
        int value = int.MaxValue;

        // Act
        var result = value.Increment();

        // Assert
        Assert.Equal(int.MaxValue, result);
    }

    [Fact]
    public void Increment_Long_ReturnsIncrementedValue()
    {
        // Arrange
        long value = 42;

        // Act
        var result = value.Increment();

        // Assert
        Assert.Equal(43, result);
    }

    [Fact]
    public void Increment_Long_MaxValue_ThrowsNotSupportedException()
    {
        // Arrange
        long value = long.MaxValue;

        // Act
        var result = value.Increment();

        // Assert
        Assert.Equal(long.MaxValue, result);
    }

    [Fact]
    public void Increment_UnsupportedType_ThrowsNotSupportedException()
    {
        // Arrange
        byte value = 42;

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => value.Increment());
    }
}