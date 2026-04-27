using Circuids.Bridge.Component.Tests.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Providers;

public sealed class BridgeConnectivityProviderComponentTests : BunitContext
{
    [Fact]
    public void BridgeConnectivityProvider_InitializesServiceAndRendersChildContent()
    {
        var recorder = new InitializationRecorder();
        var connectivity = new RecordingConnectivity(recorder);
        Services.AddSingleton<IBridgeConnectivity>(connectivity);
        var options = new ConnectivityOptions { IntervalInSeconds = 20 };

        var cut = Render<BridgeConnectivityProvider>(parameters => parameters
            .Add(component => component.Options, options)
            .AddChildContent("<span>connection</span>"));

        Assert.Equal("connection", cut.Find("span").TextContent);
        Assert.Same(options, connectivity.LastOptions);
        Assert.Equal(new[] { "Connectivity" }, recorder.Calls);
    }

    [Fact]
    public void BridgeConnectivityProvider_UsesNullOptionsByDefault()
    {
        var connectivity = new RecordingConnectivity(new InitializationRecorder());
        Services.AddSingleton<IBridgeConnectivity>(connectivity);

        Render<BridgeConnectivityProvider>(parameters => parameters
            .AddChildContent("<span>connection</span>"));

        Assert.Null(connectivity.LastOptions);
    }

    [Fact]
    public void BridgeConnectivityProvider_DoesNotRenderChildContentUntilInitializationCompletes()
    {
        var connectivity = new DelayedConnectivity();
        Services.AddSingleton<IBridgeConnectivity>(connectivity);

        var cut = Render<BridgeConnectivityProvider>(parameters => parameters
            .AddChildContent("<span>connection</span>"));

        Assert.Empty(cut.Markup.Trim());

        connectivity.CompleteInitialization();

        cut.WaitForAssertion(() => Assert.Equal("connection", cut.Find("span").TextContent));
    }

    [Fact]
    public void BridgeConnectivityProvider_InitializesServiceOnlyOnceAcrossRerenders()
    {
        var connectivity = new RecordingConnectivity(new InitializationRecorder());
        Services.AddSingleton<IBridgeConnectivity>(connectivity);

        var cut = Render<BridgeConnectivityProvider>(parameters => parameters
            .AddChildContent("<span>connection</span>"));

        cut.Render();

        Assert.Equal(1, connectivity.InitializeCallCount);
    }
}
