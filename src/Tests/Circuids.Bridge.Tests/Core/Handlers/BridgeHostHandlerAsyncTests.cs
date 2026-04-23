using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Core.Handlers;

public sealed class BridgeHostHandlerAsyncTests
{
    // ── Dispatch routing ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnBlazor_WhenHostIsBlazor()
    {
        var bridge = new FakeBridge { Host = Host.Blazor };
        var handler = new TrackedHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("Blazor", handler.CalledMethod);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnMaui_WhenHostIsMaui()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new TrackedHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("Maui", handler.CalledMethod);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnWpf_WhenHostIsWpf()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new TrackedHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("Wpf", handler.CalledMethod);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnWinForms_WhenHostIsWinForms()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new TrackedHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("WinForms", handler.CalledMethod);
    }

    // ── Default fallback behavior ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_FallsBackToOnBlazor_WhenOnWpfNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new BlazorOnlyHandler(bridge);

        await handler.ExecuteAsync();

        Assert.True(handler.BlazorCalled);
    }

    [Fact]
    public async Task ExecuteAsync_FallsBackToOnBlazor_WhenOnMauiNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new BlazorOnlyHandler(bridge);

        await handler.ExecuteAsync();

        Assert.True(handler.BlazorCalled);
    }

    // ── Unknown host ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ThrowsBridgeException_WhenHostIsUnknown()
    {
        var bridge = new FakeBridge { Host = Host.Unknown };
        var handler = new BlazorOnlyHandler(bridge);

        var act = async () => await handler.ExecuteAsync();

        await Assert.ThrowsAsync<BridgeException>(act);
    }

    [Theory]
    [InlineData(Host.Blazor)]
    [InlineData(Host.Maui)]
    [InlineData(Host.Wpf)]
    [InlineData(Host.WinForms)]
    public async Task ExecuteAsync_DoesNotThrow_ForAllKnownHostValues(Host host)
    {
        var bridge = new FakeBridge { Host = host };
        var handler = new BlazorOnlyHandler(bridge);

        var act = async () => await handler.ExecuteAsync();

        await act();
    }

    // ── Test helpers ──────────────────────────────────────────────────────────

    private sealed class TrackedHandler : BridgeHostHandlerAsync
    {
        public string CalledMethod { get; private set; } = string.Empty;

        public TrackedHandler(IBridge bridge) : base(bridge) { }

        protected override Task OnBlazor() { CalledMethod = "Blazor"; return Task.CompletedTask; }
        protected override Task OnMaui() { CalledMethod = "Maui"; return Task.CompletedTask; }
        protected override Task OnWpf() { CalledMethod = "Wpf"; return Task.CompletedTask; }
        protected override Task OnWinForms() { CalledMethod = "WinForms"; return Task.CompletedTask; }
    }

    private sealed class BlazorOnlyHandler : BridgeHostHandlerAsync
    {
        public bool BlazorCalled { get; private set; }

        public BlazorOnlyHandler(IBridge bridge) : base(bridge) { }

        protected override Task OnBlazor() { BlazorCalled = true; return Task.CompletedTask; }
    }
}
