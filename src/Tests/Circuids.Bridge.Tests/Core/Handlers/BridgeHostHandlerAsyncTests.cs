using Circuids.Bridge.TestSupport.Fakes;
using Circuids.Bridge.TestSupport.Handlers;

namespace Circuids.Bridge.Tests.Core.Handlers;

public sealed class BridgeHostHandlerAsyncTests
{
    // ── Dispatch routing ─────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnBlazor_WhenHostIsBlazor()
    {
        var bridge = new FakeBridge { Host = Host.Blazor };
        var handler = new RecordingAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("Blazor", handler.Branch);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnMaui_WhenHostIsMaui()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new RecordingAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("Maui", handler.Branch);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnWpf_WhenHostIsWpf()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new RecordingAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("Wpf", handler.Branch);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesToOnWinForms_WhenHostIsWinForms()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new RecordingAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        Assert.Equal("WinForms", handler.Branch);
    }

    // ── Default fallback behavior ─────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_FallsBackToOnBlazor_WhenOnWpfNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new BlazorOnlyAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        Assert.True(handler.BlazorCalled);
    }

    [Fact]
    public async Task ExecuteAsync_FallsBackToOnBlazor_WhenOnMauiNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new BlazorOnlyAsyncHostHandler(bridge);

        await handler.ExecuteAsync();

        Assert.True(handler.BlazorCalled);
    }

    // ── Unknown host ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ThrowsBridgeException_WhenHostIsUnknown()
    {
        var bridge = new FakeBridge { Host = Host.Unknown };
        var handler = new BlazorOnlyAsyncHostHandler(bridge);

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
        var handler = new BlazorOnlyAsyncHostHandler(bridge);

        var act = async () => await handler.ExecuteAsync();

        await act();
    }

}
