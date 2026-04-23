using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Core.Handlers;

public sealed class BridgeHostHandlerTests
{
    // ── Dispatch routing ─────────────────────────────────────────────────────

    [Fact]
    public void Execute_DispatchesToOnBlazor_WhenHostIsBlazor()
    {
        var bridge = new FakeBridge { Host = Host.Blazor };
        var handler = new TrackedHandler(bridge);

        handler.Execute();

        Assert.Equal("Blazor", handler.CalledMethod);
    }

    [Fact]
    public void Execute_DispatchesToOnMaui_WhenHostIsMaui()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new TrackedHandler(bridge);

        handler.Execute();

        Assert.Equal("Maui", handler.CalledMethod);
    }

    [Fact]
    public void Execute_DispatchesToOnWpf_WhenHostIsWpf()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new TrackedHandler(bridge);

        handler.Execute();

        Assert.Equal("Wpf", handler.CalledMethod);
    }

    [Fact]
    public void Execute_DispatchesToOnWinForms_WhenHostIsWinForms()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new TrackedHandler(bridge);

        handler.Execute();

        Assert.Equal("WinForms", handler.CalledMethod);
    }

    // ── Default fallback behavior ─────────────────────────────────────────────

    [Fact]
    public void Execute_FallsBackToOnBlazor_WhenOnWpfNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new BlazorOnlyHandler(bridge);

        handler.Execute();

        Assert.True(handler.BlazorCalled);
    }

    [Fact]
    public void Execute_FallsBackToOnBlazor_WhenOnWinFormsNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new BlazorOnlyHandler(bridge);

        handler.Execute();

        Assert.True(handler.BlazorCalled);
    }

    [Fact]
    public void Execute_FallsBackToOnBlazor_WhenOnMauiNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new BlazorOnlyHandler(bridge);

        handler.Execute();

        Assert.True(handler.BlazorCalled);
    }

    // ── Override precedence ───────────────────────────────────────────────────

    [Fact]
    public void Execute_CallsCustomOnWpf_WhenOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new TrackedHandler(bridge);

        handler.Execute();

        Assert.Equal("Wpf", handler.CalledMethod);
        Assert.False(handler.BlazorCalledAsDefault);
    }

    // ── Unknown host ──────────────────────────────────────────────────────────

    [Fact]
    public void Execute_ThrowsBridgeException_WhenHostIsUnknown()
    {
        var bridge = new FakeBridge { Host = Host.Unknown };
        var handler = new BlazorOnlyHandler(bridge);

        var act = () => handler.Execute();

        Assert.Throws<BridgeException>(act);
    }

    [Theory]
    [InlineData(Host.Blazor)]
    [InlineData(Host.Maui)]
    [InlineData(Host.Wpf)]
    [InlineData(Host.WinForms)]
    public void Execute_DoesNotThrow_ForAllKnownHostValues(Host host)
    {
        var bridge = new FakeBridge { Host = host };
        var handler = new BlazorOnlyHandler(bridge);

        var act = () => handler.Execute();

        act();
    }

    // ── Test helpers ──────────────────────────────────────────────────────────

    private sealed class TrackedHandler : BridgeHostHandler
    {
        public string CalledMethod { get; private set; } = string.Empty;
        public bool BlazorCalledAsDefault { get; private set; }

        public TrackedHandler(IBridge bridge) : base(bridge) { }

        protected override void OnBlazor() => CalledMethod = "Blazor";
        protected override void OnMaui() => CalledMethod = "Maui";
        protected override void OnWpf() => CalledMethod = "Wpf";
        protected override void OnWinForms() => CalledMethod = "WinForms";
    }

    private sealed class BlazorOnlyHandler : BridgeHostHandler
    {
        public bool BlazorCalled { get; private set; }

        public BlazorOnlyHandler(IBridge bridge) : base(bridge) { }

        protected override void OnBlazor() => BlazorCalled = true;
    }
}
