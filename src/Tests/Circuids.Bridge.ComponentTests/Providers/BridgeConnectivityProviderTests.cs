using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Providers;

public sealed class BridgeConnectivityProviderTests : BunitContext
{
    private readonly FakeBridgeConnectivity _connectivity;

    public BridgeConnectivityProviderTests()
    {
        _connectivity = new FakeBridgeConnectivity();
        Services.AddSingleton<IBridgeConnectivity>(_connectivity);
    }

    [Fact]
    public void ChildContent_IsRendered_AfterInitialization()
    {
        var cut = Render<BridgeConnectivityProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal("content", cut.Find("span").TextContent);
    }

    [Fact]
    public void InitializesConnectivityService()
    {
        Render<BridgeConnectivityProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal(1, _connectivity.InitializeCallCount);
    }

    [Fact]
    public void Options_IsPassedToConnectivityService()
    {
        var options = new ConnectivityOptions { IntervalInSeconds = 20 };

        Render<BridgeConnectivityProvider>(p => p
            .Add(c => c.Options, options)
            .AddChildContent("<span>content</span>"));

        Assert.Same(options, _connectivity.LastOptions);
    }

    [Fact]
    public void Options_IsNullByDefault()
    {
        Render<BridgeConnectivityProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Null(_connectivity.LastOptions);
    }
}
