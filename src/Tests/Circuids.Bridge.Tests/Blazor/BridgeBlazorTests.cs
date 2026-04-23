using Circuids.Bridge.Blazor.Internal;
using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Blazor;

public sealed class BridgeBlazorTests
{
    // ── Host is always Blazor ─────────────────────────────────────────────────

    [Fact]
    public void Host_IsAlwaysBlazor()
    {
        var jsRuntime = new FakeJSRuntime();
        var bridge = new BridgeBlazor(jsRuntime);

        Assert.Equal(Host.Blazor, bridge.Host);
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void IsInitialized_IsFalse_BeforeInitialize()
    {
        var jsRuntime = new FakeJSRuntime();
        var bridge = new BridgeBlazor(jsRuntime);

        Assert.False(bridge.IsInitialized);
    }

    [Fact]
    public void Platform_IsUnknown_BeforeInitialize()
    {
        var jsRuntime = new FakeJSRuntime();
        var bridge = new BridgeBlazor(jsRuntime);

        Assert.Equal(PlatformIdentity.Unknown, bridge.Platform);
    }

    // ── InitializeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_SetsIsInitializedToTrue()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getPlatform", "Windows");
        jsRuntime.Module.SetReturnValue("getPlatformVersion", "10.0.22000");

        var bridge = new BridgeBlazor(jsRuntime);
        await bridge.InitializeAsync();

        Assert.True(bridge.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_SetsPlatformFromJSResult()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getPlatform", "Windows");
        jsRuntime.Module.SetReturnValue("getPlatformVersion", "10.0.22000");

        var bridge = new BridgeBlazor(jsRuntime);
        await bridge.InitializeAsync();

        Assert.Equal(PlatformIdentity.Windows, bridge.Platform);
    }

    [Fact]
    public async Task InitializeAsync_SetsPlatformVersion_FromJSResult()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getPlatform", "Windows");
        jsRuntime.Module.SetReturnValue("getPlatformVersion", "10.0.22000");

        var bridge = new BridgeBlazor(jsRuntime);
        await bridge.InitializeAsync();

        Assert.Equal("10.0.22000", bridge.PlatformVersion);
    }

    [Fact]
    public async Task InitializeAsync_RaisesPlatformChangedEvent()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getPlatform", "Windows");
        jsRuntime.Module.SetReturnValue("getPlatformVersion", "10.0.22000");

        var bridge = new BridgeBlazor(jsRuntime);
        PlatformIdentity? raised = null;
        bridge.PlatformChanged += (_, p) => raised = p;

        await bridge.InitializeAsync();

        Assert.Equal(PlatformIdentity.Windows, raised);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent_CallsJSOnlyOnce()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getPlatform", "Windows");
        jsRuntime.Module.SetReturnValue("getPlatformVersion", "10.0.22000");

        var bridge = new BridgeBlazor(jsRuntime);
        await bridge.InitializeAsync();
        await bridge.InitializeAsync();

        var getPlatformCalls = jsRuntime.Module.Invocations.Count(i => i.Identifier == "getPlatform");
        Assert.Equal(1, getPlatformCalls);
    }

    [Theory]
    [InlineData("Android", PlatformIdentity.Android)]
    [InlineData("IOS", PlatformIdentity.IOS)]
    [InlineData("Windows", PlatformIdentity.Windows)]
    [InlineData("Mac", PlatformIdentity.Mac)]
    [InlineData("Linux", PlatformIdentity.Linux)]
    public async Task InitializeAsync_ParsesAllPlatformIdentities(string jsValue, PlatformIdentity expected)
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getPlatform", jsValue);
        jsRuntime.Module.SetReturnValue("getPlatformVersion", "1.0");

        var bridge = new BridgeBlazor(jsRuntime);
        await bridge.InitializeAsync();

        Assert.Equal(expected, bridge.Platform);
    }

    // ── DisposeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_WhenNotInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        var bridge = new BridgeBlazor(jsRuntime);

        var act = async () => await bridge.DisposeAsync();

        await act();
    }

    [Fact]
    public async Task DisposeAsync_DisposesJSModule_WhenInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getPlatform", "Windows");
        jsRuntime.Module.SetReturnValue("getPlatformVersion", "10.0.22000");

        var bridge = new BridgeBlazor(jsRuntime);
        await bridge.InitializeAsync();
        await bridge.DisposeAsync();

        Assert.True(jsRuntime.Module.IsDisposed);
    }
}
