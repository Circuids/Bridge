using System.Text.Json;
using Circuids.Bridge.Blazor.Internal;
using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Blazor;

public sealed class BridgeSafeAreaBlazorTests
{
    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void SafeArea_IsZero_BeforeInitialize()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeSafeAreaBlazor(jsRuntime);

        Assert.Equal(SafeAreaInsets.Zero, service.SafeArea);
    }

    // ── InitializeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_SetsSafeAreaFromJSResult()
    {
        var jsRuntime = new FakeJSRuntime();
        var insets = new SafeAreaInsets(44, 0, 34, 0);
        jsRuntime.Module.SetReturnValue("getSafeAreaInsets", JsonSerializer.Serialize(insets));

        var service = new BridgeSafeAreaBlazor(jsRuntime);
        await service.InitializeAsync();

        Assert.Equal(insets, service.SafeArea);
    }

    [Fact]
    public async Task InitializeAsync_UsesSafeAreaZero_WhenJSReturnsEmpty()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getSafeAreaInsets", string.Empty);

        var service = new BridgeSafeAreaBlazor(jsRuntime);
        await service.InitializeAsync();

        Assert.Equal(SafeAreaInsets.Zero, service.SafeArea);
    }

    [Fact]
    public async Task InitializeAsync_RaisesSafeAreaChangedEvent()
    {
        var jsRuntime = new FakeJSRuntime();
        var insets = new SafeAreaInsets(44, 0, 34, 0);
        jsRuntime.Module.SetReturnValue("getSafeAreaInsets", JsonSerializer.Serialize(insets));

        var service = new BridgeSafeAreaBlazor(jsRuntime);
        SafeAreaInsets? raised = null;
        service.SafeAreaChanged += (_, s) => raised = s;

        await service.InitializeAsync();

        Assert.Equal(insets, raised);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getSafeAreaInsets", JsonSerializer.Serialize(SafeAreaInsets.Zero));

        var service = new BridgeSafeAreaBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.InitializeAsync();

        var getCalls = jsRuntime.Module.Invocations.Count(i => i.Identifier == "getSafeAreaInsets");
        Assert.Equal(1, getCalls);
    }

    // ── NotifySafeAreaChanged callback ────────────────────────────────────────

    [Fact]
    public async Task NotifySafeAreaChanged_UpdatesSafeArea_WhenValueChanges()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getSafeAreaInsets", JsonSerializer.Serialize(SafeAreaInsets.Zero));

        var service = new BridgeSafeAreaBlazor(jsRuntime);
        await service.InitializeAsync();

        var newInsets = new SafeAreaInsets(44, 0, 34, 0);
        service.NotifySafeAreaChanged(JsonSerializer.Serialize(newInsets));

        Assert.Equal(newInsets, service.SafeArea);
    }

    [Fact]
    public async Task NotifySafeAreaChanged_RaisesSafeAreaChangedEvent_WhenValueChanges()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getSafeAreaInsets", JsonSerializer.Serialize(SafeAreaInsets.Zero));

        var service = new BridgeSafeAreaBlazor(jsRuntime);
        await service.InitializeAsync();

        SafeAreaInsets? raised = null;
        service.SafeAreaChanged += (_, s) => raised = s;

        var newInsets = new SafeAreaInsets(44, 0, 34, 0);
        service.NotifySafeAreaChanged(JsonSerializer.Serialize(newInsets));

        Assert.Equal(newInsets, raised);
    }

    // ── DisposeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_WhenNotInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeSafeAreaBlazor(jsRuntime);

        var act = async () => await service.DisposeAsync();

        await act();
    }

    [Fact]
    public async Task DisposeAsync_DisposesJSModule_WhenInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getSafeAreaInsets", JsonSerializer.Serialize(SafeAreaInsets.Zero));

        var service = new BridgeSafeAreaBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.DisposeAsync();

        Assert.True(jsRuntime.Module.IsDisposed);
    }
}
