using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Components;

public sealed class BridgeConnectivityTests : BunitContext
{
    private readonly FakeBridgeConnectivity _connectivity;

    public BridgeConnectivityTests()
    {
        _connectivity = new FakeBridgeConnectivity();
        Services.AddSingleton<IBridgeConnectivity>(_connectivity);
    }

    [Fact]
    public void Renders_OnlineSlot_WhenIsConnectedIsTrue()
    {
        _connectivity.IsConnected = true;

        var cut = Render<BridgeConnectivity>(p => p
            .Add(c => c.Online, "<span>online</span>")
            .Add(c => c.Offline, "<span>offline</span>"));

        Assert.Equal("online", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_OfflineSlot_WhenIsConnectedIsFalse()
    {
        _connectivity.IsConnected = false;

        var cut = Render<BridgeConnectivity>(p => p
            .Add(c => c.Online, "<span>online</span>")
            .Add(c => c.Offline, "<span>offline</span>"));

        Assert.Equal("offline", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_ChildContent_WithCurrentConnectionState()
    {
        _connectivity.IsConnected = true;

        var cut = Render<BridgeConnectivity>(p => p
            .Add(c => c.ChildContent, (bool online) => $"<span>{online}</span>"));

        Assert.Equal("True", cut.Find("span").TextContent);
    }

    [Fact]
    public void Re_Renders_WhenConnectionChanges_ToOffline()
    {
        _connectivity.IsConnected = true;

        var cut = Render<BridgeConnectivity>(p => p
            .Add(c => c.Online, "<span>online</span>")
            .Add(c => c.Offline, "<span>offline</span>"));

        _connectivity.RaiseConnectionChanged(false);

        Assert.Equal("offline", cut.Find("span").TextContent);
    }

    [Fact]
    public void Re_Renders_WhenConnectionChanges_ToOnline()
    {
        _connectivity.IsConnected = false;

        var cut = Render<BridgeConnectivity>(p => p
            .Add(c => c.Online, "<span>online</span>")
            .Add(c => c.Offline, "<span>offline</span>"));

        _connectivity.RaiseConnectionChanged(true);

        Assert.Equal("online", cut.Find("span").TextContent);
    }
}
