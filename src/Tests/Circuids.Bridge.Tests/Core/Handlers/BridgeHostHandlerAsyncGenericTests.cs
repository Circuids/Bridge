using Circuids.Bridge.Tests.Fakes;

namespace Circuids.Bridge.Tests.Core.Handlers;

public sealed class BridgeHostHandlerAsyncGenericTests
{
    // ── Dispatch routing ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ReturnsBlazorResult_WhenHostIsBlazor()
    {
        var bridge = new FakeBridge { Host = Host.Blazor };
        var handler = new TrackedHandler(bridge);

        var result = await handler.ExecuteAsync();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMauiResult_WhenHostIsMaui()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new TrackedHandler(bridge);

        var result = await handler.ExecuteAsync();

        Assert.Equal("Maui", result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsWpfResult_WhenHostIsWpf()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new TrackedHandler(bridge);

        var result = await handler.ExecuteAsync();

        Assert.Equal("Wpf", result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsWinFormsResult_WhenHostIsWinForms()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new TrackedHandler(bridge);

        var result = await handler.ExecuteAsync();

        Assert.Equal("WinForms", result);
    }

    // ── Default fallback behavior ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ReturnsBlazorValue_WhenOnWpfNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new BlazorOnlyHandler(bridge);

        var result = await handler.ExecuteAsync();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBlazorValue_WhenOnMauiNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new BlazorOnlyHandler(bridge);

        var result = await handler.ExecuteAsync();

        Assert.Equal("Blazor", result);
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

    // ── Test helpers ──────────────────────────────────────────────────────────

    private sealed class TrackedHandler : BridgeHostHandlerAsync<string>
    {
        public TrackedHandler(IBridge bridge) : base(bridge) { }

        protected override Task<string> OnBlazor() => Task.FromResult("Blazor");
        protected override Task<string> OnMaui() => Task.FromResult("Maui");
        protected override Task<string> OnWpf() => Task.FromResult("Wpf");
        protected override Task<string> OnWinForms() => Task.FromResult("WinForms");
    }

    private sealed class BlazorOnlyHandler : BridgeHostHandlerAsync<string>
    {
        public BlazorOnlyHandler(IBridge bridge) : base(bridge) { }

        protected override Task<string> OnBlazor() => Task.FromResult("Blazor");
    }
}
