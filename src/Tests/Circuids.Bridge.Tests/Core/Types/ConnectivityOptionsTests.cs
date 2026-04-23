namespace Circuids.Bridge.Tests.Core.Types;

public sealed class ConnectivityOptionsTests
{
    [Fact]
    public void DefaultIntervalInSeconds_IsTen()
    {
        var options = new ConnectivityOptions();

        Assert.Equal(10, options.IntervalInSeconds);
    }

    [Fact]
    public void DefaultTestUrl_IsFaviconIco()
    {
        var options = new ConnectivityOptions();

        Assert.Equal("/favicon.ico", options.TestUrl);
    }

    [Fact]
    public void IntervalInSeconds_CanBeSet()
    {
        var options = new ConnectivityOptions { IntervalInSeconds = 30 };

        Assert.Equal(30, options.IntervalInSeconds);
    }

    [Fact]
    public void TestUrl_CanBeSet()
    {
        var options = new ConnectivityOptions { TestUrl = "/health" };

        Assert.Equal("/health", options.TestUrl);
    }
}
