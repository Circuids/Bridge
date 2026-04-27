using Circuids.Bridge.TestSupport.Fakes;
using Circuids.Bridge.TestSupport.Handlers;

namespace Circuids.Bridge.Tests.Core.Handlers;

public sealed class BridgeHostHandlerTests
{
    // ── Dispatch routing ─────────────────────────────────────────────────────

    [Fact]
    public void Execute_DispatchesToOnBlazor_WhenHostIsBlazor()
    {
        var bridge = new FakeBridge { Host = Host.Blazor };
        var handler = new RecordingHostHandler(bridge);

        handler.Execute();

        Assert.Equal("Blazor", handler.Branch);
    }

    [Fact]
    public void Execute_DispatchesToOnMaui_WhenHostIsMaui()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new RecordingHostHandler(bridge);

        handler.Execute();

        Assert.Equal("Maui", handler.Branch);
    }

    [Fact]
    public void Execute_DispatchesToOnWpf_WhenHostIsWpf()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new RecordingHostHandler(bridge);

        handler.Execute();

        Assert.Equal("Wpf", handler.Branch);
    }

    [Fact]
    public void Execute_DispatchesToOnWinForms_WhenHostIsWinForms()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new RecordingHostHandler(bridge);

        handler.Execute();

        Assert.Equal("WinForms", handler.Branch);
    }

    // ── Default fallback behavior ─────────────────────────────────────────────

    [Fact]
    public void Execute_FallsBackToOnBlazor_WhenOnWpfNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new BlazorOnlyActionHostHandler(bridge);

        handler.Execute();

        Assert.True(handler.BlazorCalled);
    }

    [Fact]
    public void Execute_FallsBackToOnBlazor_WhenOnWinFormsNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new BlazorOnlyActionHostHandler(bridge);

        handler.Execute();

        Assert.True(handler.BlazorCalled);
    }

    [Fact]
    public void Execute_FallsBackToOnBlazor_WhenOnMauiNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new BlazorOnlyActionHostHandler(bridge);

        handler.Execute();

        Assert.True(handler.BlazorCalled);
    }

    // ── Override precedence ───────────────────────────────────────────────────

    [Fact]
    public void Execute_CallsCustomOnWpf_WhenOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new RecordingHostHandler(bridge);

        handler.Execute();

        Assert.Equal("Wpf", handler.Branch);
    }

    // ── Unknown host ──────────────────────────────────────────────────────────

    [Fact]
    public void Execute_ThrowsBridgeException_WhenHostIsUnknown()
    {
        var bridge = new FakeBridge { Host = Host.Unknown };
        var handler = new BlazorOnlyActionHostHandler(bridge);

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
        var handler = new BlazorOnlyActionHostHandler(bridge);

        var act = () => handler.Execute();

        act();
    }

}
