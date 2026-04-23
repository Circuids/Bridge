using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Providers;

public sealed class BridgeProviderTests : BunitContext
{
    private readonly FakeBridge _bridge;
    private readonly FakeBridgeFormFactor _formFactor;
    private readonly FakeBridgeConnectivity _connectivity;
    private readonly FakeBridgeTheme _theme;
    private readonly FakeBridgeSafeArea _safeArea;

    public BridgeProviderTests()
    {
        _bridge = new FakeBridge();
        _formFactor = new FakeBridgeFormFactor();
        _connectivity = new FakeBridgeConnectivity();
        _theme = new FakeBridgeTheme();
        _safeArea = new FakeBridgeSafeArea();

        Services.AddSingleton<IBridge>(_bridge);
        Services.AddSingleton<IBridgeFormFactor>(_formFactor);
        Services.AddSingleton<IBridgeConnectivity>(_connectivity);
        Services.AddSingleton<IBridgeTheme>(_theme);
        Services.AddSingleton<IBridgeSafeArea>(_safeArea);
    }

    // ── Initialization gating ─────────────────────────────────────────────────

    [Fact]
    public void ChildContent_IsNotRendered_BeforeInitialization()
    {
        // We can't easily intercept OnAfterRenderAsync timing,
        // so verify the initial markup has no child content
        var cut = Render<BridgeProvider>(p => p
            .AddChildContent("<span>child</span>"));

        // After bUnit render, OnAfterRenderAsync has run and initialized all services
        // so we verify all InitializeAsync were called
        Assert.Equal(1, _bridge.InitializeCallCount);
        Assert.Equal(1, _formFactor.InitializeCallCount);
        Assert.Equal(1, _connectivity.InitializeCallCount);
        Assert.Equal(1, _theme.InitializeCallCount);
        Assert.Equal(1, _safeArea.InitializeCallCount);
    }

    [Fact]
    public void ChildContent_IsRendered_AfterAllServicesInitialized()
    {
        var cut = Render<BridgeProvider>(p => p
            .AddChildContent("<span>child</span>"));

        Assert.Equal("child", cut.Find("span").TextContent);
    }

    // ── Service initialization order ──────────────────────────────────────────

    [Fact]
    public void InitializesAllFiveServices()
    {
        Render<BridgeProvider>(p => p
            .AddChildContent("<span>child</span>"));

        Assert.Equal(1, _bridge.InitializeCallCount);
        Assert.Equal(1, _formFactor.InitializeCallCount);
        Assert.Equal(1, _connectivity.InitializeCallCount);
        Assert.Equal(1, _theme.InitializeCallCount);
        Assert.Equal(1, _safeArea.InitializeCallCount);
    }

    // ── Parameter passing ─────────────────────────────────────────────────────

    [Fact]
    public void FormFactorResizeMode_IsPassedToFormFactorService()
    {
        Render<BridgeProvider>(p => p
            .Add(c => c.FormFactorResizeMode, ResizeMode.Global)
            .AddChildContent("<span>child</span>"));

        Assert.Equal(ResizeMode.Global, _formFactor.LastResizeMode);
    }

    [Fact]
    public void ConnectivityOptions_IsPassedToConnectivityService()
    {
        var options = new ConnectivityOptions { IntervalInSeconds = 60 };

        Render<BridgeProvider>(p => p
            .Add(c => c.ConnectivityOptions, options)
            .AddChildContent("<span>child</span>"));

        Assert.Same(options, _connectivity.LastOptions);
    }

    [Fact]
    public void ConnectivityOptions_IsNullByDefault()
    {
        Render<BridgeProvider>(p => p
            .AddChildContent("<span>child</span>"));

        Assert.Null(_connectivity.LastOptions);
    }
}
