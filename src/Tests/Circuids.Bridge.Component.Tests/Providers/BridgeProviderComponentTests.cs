using Circuids.Bridge.Component.Tests.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Providers;

public sealed class BridgeProviderComponentTests : BunitContext
{
    [Fact]
    public void BridgeProvider_InitializesAllServices_InDocumentedOrder()
    {
        var recorder = new InitializationRecorder();
        var bridge = new RecordingBridge(recorder);
        var formFactor = new RecordingFormFactor(recorder);
        var connectivity = new RecordingConnectivity(recorder);
        var theme = new RecordingTheme(recorder);
        var safeArea = new RecordingSafeArea(recorder);

        Services.AddSingleton<IBridge>(bridge);
        Services.AddSingleton<IBridgeFormFactor>(formFactor);
        Services.AddSingleton<IBridgeConnectivity>(connectivity);
        Services.AddSingleton<IBridgeTheme>(theme);
        Services.AddSingleton<IBridgeSafeArea>(safeArea);

        var options = new ConnectivityOptions { IntervalInSeconds = 30, TestUrl = "/health" };

        Render<BridgeProvider>(parameters => parameters
            .Add(component => component.FormFactorResizeMode, ResizeMode.Global)
            .Add(component => component.ConnectivityOptions, options)
            .AddChildContent("<span>ready</span>"));

        Assert.Equal(
            new[] { "Bridge", "FormFactor", "Connectivity", "Theme", "SafeArea" },
            recorder.Calls);
        Assert.Equal(ResizeMode.Global, formFactor.LastResizeMode);
        Assert.Same(options, connectivity.LastOptions);
    }

    [Fact]
    public void BridgeProvider_UsesDefaultFormFactorModeAndNullConnectivityOptions()
    {
        var recorder = new InitializationRecorder();
        var formFactor = new RecordingFormFactor(recorder);
        var connectivity = new RecordingConnectivity(recorder);

        Services.AddSingleton<IBridge>(new RecordingBridge(recorder));
        Services.AddSingleton<IBridgeFormFactor>(formFactor);
        Services.AddSingleton<IBridgeConnectivity>(connectivity);
        Services.AddSingleton<IBridgeTheme>(new RecordingTheme(recorder));
        Services.AddSingleton<IBridgeSafeArea>(new RecordingSafeArea(recorder));

        Render<BridgeProvider>(parameters => parameters
            .AddChildContent("<span>ready</span>"));

        Assert.Equal(ResizeMode.None, formFactor.LastResizeMode);
        Assert.Null(connectivity.LastOptions);
    }

    [Fact]
    public void BridgeProvider_RendersChildContent_AfterInitialization()
    {
        RegisterRecordingServices(new InitializationRecorder());

        var cut = Render<BridgeProvider>(parameters => parameters
            .AddChildContent("<span>ready</span>"));

        Assert.Equal("ready", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeProvider_DoesNotRenderChildContentUntilInitializationCompletes()
    {
        var recorder = new InitializationRecorder();
        var bridge = new DelayedBridge();

        Services.AddSingleton<IBridge>(bridge);
        Services.AddSingleton<IBridgeFormFactor>(new RecordingFormFactor(recorder));
        Services.AddSingleton<IBridgeConnectivity>(new RecordingConnectivity(recorder));
        Services.AddSingleton<IBridgeTheme>(new RecordingTheme(recorder));
        Services.AddSingleton<IBridgeSafeArea>(new RecordingSafeArea(recorder));

        var cut = Render<BridgeProvider>(parameters => parameters
            .AddChildContent("<span>ready</span>"));

        Assert.Empty(cut.Markup.Trim());

        bridge.CompleteInitialization();

        cut.WaitForAssertion(() => Assert.Equal("ready", cut.Find("span").TextContent));
    }

    [Fact]
    public void BridgeProvider_InitializesEachServiceOnlyOnceAcrossRerenders()
    {
        var recorder = new InitializationRecorder();
        var bridge = new RecordingBridge(recorder);
        var formFactor = new RecordingFormFactor(recorder);
        var connectivity = new RecordingConnectivity(recorder);
        var theme = new RecordingTheme(recorder);
        var safeArea = new RecordingSafeArea(recorder);

        Services.AddSingleton<IBridge>(bridge);
        Services.AddSingleton<IBridgeFormFactor>(formFactor);
        Services.AddSingleton<IBridgeConnectivity>(connectivity);
        Services.AddSingleton<IBridgeTheme>(theme);
        Services.AddSingleton<IBridgeSafeArea>(safeArea);

        var cut = Render<BridgeProvider>(parameters => parameters
            .AddChildContent("<span>ready</span>"));

        cut.Render();

        Assert.Equal(1, bridge.InitializeCallCount);
        Assert.Equal(1, formFactor.InitializeCallCount);
        Assert.Equal(1, connectivity.InitializeCallCount);
        Assert.Equal(1, theme.InitializeCallCount);
        Assert.Equal(1, safeArea.InitializeCallCount);
    }

    private void RegisterRecordingServices(InitializationRecorder recorder)
    {
        Services.AddSingleton<IBridge>(new RecordingBridge(recorder));
        Services.AddSingleton<IBridgeFormFactor>(new RecordingFormFactor(recorder));
        Services.AddSingleton<IBridgeConnectivity>(new RecordingConnectivity(recorder));
        Services.AddSingleton<IBridgeTheme>(new RecordingTheme(recorder));
        Services.AddSingleton<IBridgeSafeArea>(new RecordingSafeArea(recorder));
    }
}
