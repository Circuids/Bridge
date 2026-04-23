using Circuids.Bridge.Blazor.Internal;
using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Blazor;

public sealed class BridgeThemeBlazorTests
{
    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void Theme_IsUnknown_BeforeInitialize()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeThemeBlazor(jsRuntime);

        Assert.Equal(ThemeMode.Unknown, service.Theme);
    }

    // ── InitializeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_SetsThemeFromJSResult()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "dark");

        var service = new BridgeThemeBlazor(jsRuntime);
        await service.InitializeAsync();

        Assert.Equal(ThemeMode.Dark, service.Theme);
    }

    [Fact]
    public async Task InitializeAsync_ParsesThemeCaseInsensitively()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "LIGHT");

        var service = new BridgeThemeBlazor(jsRuntime);
        await service.InitializeAsync();

        Assert.Equal(ThemeMode.Light, service.Theme);
    }

    [Fact]
    public async Task InitializeAsync_RaisesThemeChangedEvent()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "light");

        var service = new BridgeThemeBlazor(jsRuntime);
        ThemeMode? raised = null;
        service.ThemeChanged += (_, t) => raised = t;

        await service.InitializeAsync();

        Assert.Equal(ThemeMode.Light, raised);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "dark");

        var service = new BridgeThemeBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.InitializeAsync();

        var getThemeCalls = jsRuntime.Module.Invocations.Count(i => i.Identifier == "getTheme");
        Assert.Equal(1, getThemeCalls);
    }

    [Fact]
    public async Task InitializeAsync_CallsInitializeListener()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "dark");

        var service = new BridgeThemeBlazor(jsRuntime);
        await service.InitializeAsync();

        Assert.Contains(jsRuntime.Module.Invocations, i => i.Identifier == "initializeListener");
    }

    // ── NotifyThemeChanged callback ───────────────────────────────────────────

    [Fact]
    public async Task NotifyThemeChanged_UpdatesTheme_WhenValueChanges()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "light");

        var service = new BridgeThemeBlazor(jsRuntime);
        await service.InitializeAsync();

        service.NotifyThemeChanged("dark");

        Assert.Equal(ThemeMode.Dark, service.Theme);
    }

    [Fact]
    public async Task NotifyThemeChanged_RaisesThemeChangedEvent_WhenValueChanges()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "light");

        var service = new BridgeThemeBlazor(jsRuntime);
        ThemeMode? raised = null;
        service.ThemeChanged += (_, t) => raised = t;

        await service.InitializeAsync();
        raised = null; // reset from init raise

        service.NotifyThemeChanged("dark");

        Assert.Equal(ThemeMode.Dark, raised);
    }

    [Fact]
    public async Task NotifyThemeChanged_DoesNotRaiseEvent_WhenSameValue()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "light");

        var service = new BridgeThemeBlazor(jsRuntime);
        await service.InitializeAsync();

        var eventCount = 0;
        service.ThemeChanged += (_, _) => eventCount++;

        service.NotifyThemeChanged("light"); // same value

        Assert.Equal(0, eventCount);
    }

    // ── DisposeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_WhenNotInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeThemeBlazor(jsRuntime);

        var act = async () => await service.DisposeAsync();

        await act();
    }

    [Fact]
    public async Task DisposeAsync_DisposesJSModule_WhenInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getTheme", "dark");

        var service = new BridgeThemeBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.DisposeAsync();

        Assert.True(jsRuntime.Module.IsDisposed);
    }
}
