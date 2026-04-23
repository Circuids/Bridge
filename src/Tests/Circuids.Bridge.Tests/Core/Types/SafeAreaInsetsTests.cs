namespace Circuids.Bridge.Tests.Core.Types;

public sealed class SafeAreaInsetsTests
{
    [Fact]
    public void Zero_ReturnsAllZeroInsets()
    {
        var insets = SafeAreaInsets.Zero;

        Assert.Equal(0, insets.Top);
        Assert.Equal(0, insets.Right);
        Assert.Equal(0, insets.Bottom);
        Assert.Equal(0, insets.Left);
    }

    [Fact]
    public void Zero_HasInsets_ReturnsFalse()
    {
        Assert.False(SafeAreaInsets.Zero.HasInsets);
    }

    [Theory]
    [InlineData(44, 0, 0, 0)]
    [InlineData(0, 10, 0, 0)]
    [InlineData(0, 0, 34, 0)]
    [InlineData(0, 0, 0, 8)]
    public void HasInsets_ReturnsTrue_WhenAnyInsetIsNonZero(double top, double right, double bottom, double left)
    {
        var insets = new SafeAreaInsets(top, right, bottom, left);

        Assert.True(insets.HasInsets);
    }

    [Fact]
    public void Constructor_StoresAllValues()
    {
        var insets = new SafeAreaInsets(44, 0, 34, 0);

        Assert.Equal(44, insets.Top);
        Assert.Equal(0, insets.Right);
        Assert.Equal(34, insets.Bottom);
        Assert.Equal(0, insets.Left);
    }

    [Fact]
    public void TwoInstances_WithSameValues_AreEqual()
    {
        var a = new SafeAreaInsets(44, 0, 34, 0);
        var b = new SafeAreaInsets(44, 0, 34, 0);

        Assert.Equal(b, a);
    }

    [Fact]
    public void TwoInstances_WithDifferentValues_AreNotEqual()
    {
        var a = new SafeAreaInsets(44, 0, 34, 0);
        var b = new SafeAreaInsets(0, 0, 0, 0);

        Assert.NotEqual(b, a);
    }
}
