using Circuids.Bridge.TestSupport.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Components;

public sealed class BridgeConnectivityComponentTests : BunitContext
{
    [Theory]
    [InlineData(true, "online")]
    [InlineData(false, "offline")]
    public void BridgeConnectivity_RendersCurrentConnectionState(bool isConnected, string expected)
    {
        var connectivity = new FakeBridgeConnectivity { IsConnected = isConnected };
        Services.AddSingleton<IBridgeConnectivity>(connectivity);

        var cut = Render<BridgeConnectivity>(parameters => parameters
            .Add(component => component.Online, "<span>online</span>")
            .Add(component => component.Offline, "<span>offline</span>"));

        Assert.Equal(expected, cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeConnectivity_RendersChildContentBeforeStatusSlot()
    {
        var connectivity = new FakeBridgeConnectivity { IsConnected = true };
        Services.AddSingleton<IBridgeConnectivity>(connectivity);

        var cut = Render<BridgeConnectivity>(parameters => parameters
            .Add(component => component.Online, "<span>online</span>")
            .Add(component => component.Offline, "<span>offline</span>")
            .Add(component => component.ChildContent, (bool online) => $"<strong>{online}</strong>"));

        Assert.Equal("True", cut.Find("strong").TextContent);
        Assert.Equal("online", cut.Find("span").TextContent);
        Assert.Equal("strong", cut.Nodes[0].NodeName.ToLowerInvariant());
    }

    [Theory]
    [InlineData(true, false, "offline")]
    [InlineData(false, true, "online")]
    public void BridgeConnectivity_RerendersOnConnectionChange(bool initial, bool changed, string expected)
    {
        var connectivity = new FakeBridgeConnectivity { IsConnected = initial };
        Services.AddSingleton<IBridgeConnectivity>(connectivity);

        var cut = Render<BridgeConnectivity>(parameters => parameters
            .Add(component => component.Online, "<span>online</span>")
            .Add(component => component.Offline, "<span>offline</span>"));

        connectivity.RaiseConnectionChanged(changed);

        cut.WaitForAssertion(() => Assert.Equal(expected, cut.Find("span").TextContent));
    }

    [Fact]
    public void BridgeConnectivity_InvokesIsConnectedChangedOnInitialRenderAndChanges()
    {
        var connectivity = new FakeBridgeConnectivity { IsConnected = true };
        Services.AddSingleton<IBridgeConnectivity>(connectivity);
        var values = new List<bool>();

        Render<BridgeConnectivity>(parameters => parameters
            .Add(component => component.Online, "<span>online</span>")
            .Add(component => component.Offline, "<span>offline</span>")
            .Add(component => component.IsConnectedChanged, values.Add));

        connectivity.RaiseConnectionChanged(false);

        Assert.Equal(new[] { true, false }, values);
    }

    [Fact]
    public void BridgeConnectivity_DoesNotInvokeIsConnectedChangedForSameValueEvent()
    {
        var connectivity = new FakeBridgeConnectivity { IsConnected = true };
        Services.AddSingleton<IBridgeConnectivity>(connectivity);
        var values = new List<bool>();

        Render<BridgeConnectivity>(parameters => parameters
            .Add(component => component.Online, "<span>online</span>")
            .Add(component => component.Offline, "<span>offline</span>")
            .Add(component => component.IsConnectedChanged, values.Add));

        connectivity.RaiseConnectionChanged(true);

        Assert.Equal(new[] { true }, values);
    }

    [Fact]
    public void BridgeConnectivity_UnsubscribesFromConnectionChangedOnDispose()
    {
        var connectivity = new FakeBridgeConnectivity { IsConnected = true };
        Services.AddSingleton<IBridgeConnectivity>(connectivity);

        var cut = Render<BridgeConnectivity>(parameters => parameters
            .Add(component => component.Online, "<span>online</span>")
            .Add(component => component.Offline, "<span>offline</span>"));

        cut.Dispose();
        connectivity.RaiseConnectionChanged(false);
    }
}
