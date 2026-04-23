using Circuids.Bridge.Blazor.Internal;
using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Blazor;

public sealed class BridgeConnectivityBlazorTests
{
    // ── Initial state ─────────────────────────────────────────────────────────

    [Fact]
    public void IsConnected_IsTrue_BeforeInitialize()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeConnectivityBlazor(jsRuntime);

        Assert.True(service.IsConnected);
    }

    // ── InitializeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_SetsIsConnectedFromJSResult()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", false);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync();

        Assert.False(service.IsConnected);
    }

    [Fact]
    public async Task InitializeAsync_RaisesConnectionChangedEvent()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        bool? raised = null;
        service.ConnectionChanged += (_, c) => raised = c;

        await service.InitializeAsync();

        Assert.True(raised);
    }

    [Fact]
    public async Task InitializeAsync_UsesDefaultOptions_WhenNullPassed()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync(null);

        var initListenerCall = jsRuntime.Module.Invocations.FirstOrDefault(i => i.Identifier == "initializeListener");
        Assert.NotNull(initListenerCall.Identifier); // assert the call was made
        Assert.Contains(10, initListenerCall.Args);    // default IntervalInSeconds
        Assert.Contains("/favicon.ico", initListenerCall.Args); // default TestUrl
    }

    [Fact]
    public async Task InitializeAsync_UsesCustomOptions_WhenProvided()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var options = new ConnectivityOptions { IntervalInSeconds = 30, TestUrl = "/health" };
        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync(options);

        var initListenerCall = jsRuntime.Module.Invocations.FirstOrDefault(i => i.Identifier == "initializeListener");
        Assert.Contains(30, initListenerCall.Args);
        Assert.Contains("/health", initListenerCall.Args);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.InitializeAsync();

        var statusCalls = jsRuntime.Module.Invocations.Count(i => i.Identifier == "getNetworkStatus");
        Assert.Equal(1, statusCalls);
    }

    // ── NotifyConnectivityStatusChanged callback ──────────────────────────────

    [Fact]
    public async Task NotifyConnectivityStatusChanged_UpdatesIsConnected()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync();

        service.NotifyConnectivityStatusChanged(false);

        Assert.False(service.IsConnected);
    }

    [Fact]
    public async Task NotifyConnectivityStatusChanged_RaisesConnectionChangedEvent_WhenValueChanges()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync();

        bool? raised = null;
        service.ConnectionChanged += (_, c) => raised = c;

        service.NotifyConnectivityStatusChanged(false);

        Assert.False(raised);
    }

    [Fact]
    public async Task NotifyConnectivityStatusChanged_DoesNotRaiseEvent_WhenSameValue()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync();

        var eventCount = 0;
        service.ConnectionChanged += (_, _) => eventCount++;

        service.NotifyConnectivityStatusChanged(true); // same value

        Assert.Equal(0, eventCount);
    }

    // ── DisposeAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisposeAsync_DoesNotThrow_WhenNotInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BridgeConnectivityBlazor(jsRuntime);

        var act = async () => await service.DisposeAsync();

        await act();
    }

    [Fact]
    public async Task DisposeAsync_DisposesJSModule_WhenInitialized()
    {
        var jsRuntime = new FakeJSRuntime();
        jsRuntime.Module.SetReturnValue("getNetworkStatus", true);

        var service = new BridgeConnectivityBlazor(jsRuntime);
        await service.InitializeAsync();
        await service.DisposeAsync();

        Assert.True(jsRuntime.Module.IsDisposed);
    }
}
