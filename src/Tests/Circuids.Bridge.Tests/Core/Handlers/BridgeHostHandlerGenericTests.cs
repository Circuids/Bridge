using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Core.Handlers;

public sealed class BridgeHostHandlerGenericTests
{
    // ── Dispatch routing ─────────────────────────────────────────────────────

    [Fact]
    public void Execute_ReturnsBlazorResult_WhenHostIsBlazor()
    {
        var bridge = new FakeBridge { Host = Host.Blazor };
        var handler = new TrackedHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public void Execute_ReturnsMauiResult_WhenHostIsMaui()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new TrackedHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Maui", result);
    }

    [Fact]
    public void Execute_ReturnsWpfResult_WhenHostIsWpf()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new TrackedHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Wpf", result);
    }

    [Fact]
    public void Execute_ReturnsWinFormsResult_WhenHostIsWinForms()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new TrackedHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("WinForms", result);
    }

    // ── Default fallback behavior ─────────────────────────────────────────────

    [Fact]
    public void Execute_ReturnsBlazorValue_WhenOnWpfNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new BlazorOnlyHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public void Execute_ReturnsBlazorValue_WhenOnWinFormsNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new BlazorOnlyHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public void Execute_ReturnsBlazorValue_WhenOnMauiNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new BlazorOnlyHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
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

    // ── Test helpers ──────────────────────────────────────────────────────────

    private sealed class TrackedHandler : BridgeHostHandler<string>
    {
        public TrackedHandler(IBridge bridge) : base(bridge) { }

        protected override string OnBlazor() => "Blazor";
        protected override string OnMaui() => "Maui";
        protected override string OnWpf() => "Wpf";
        protected override string OnWinForms() => "WinForms";
    }

    private sealed class BlazorOnlyHandler : BridgeHostHandler<string>
    {
        public BlazorOnlyHandler(IBridge bridge) : base(bridge) { }

        protected override string OnBlazor() => "Blazor";
    }
}
