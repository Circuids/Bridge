using Circuids.Bridge.TestSupport.Fakes;
using Circuids.Bridge.TestSupport.Handlers;

namespace Circuids.Bridge.Tests.Core.Handlers;

public sealed class BridgeHostHandlerGenericTests
{
    // ── Dispatch routing ─────────────────────────────────────────────────────

    [Fact]
    public void Execute_ReturnsBlazorResult_WhenHostIsBlazor()
    {
        var bridge = new FakeBridge { Host = Host.Blazor };
        var handler = new ReturningHostHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public void Execute_ReturnsMauiResult_WhenHostIsMaui()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new ReturningHostHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Maui", result);
    }

    [Fact]
    public void Execute_ReturnsWpfResult_WhenHostIsWpf()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new ReturningHostHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Wpf", result);
    }

    [Fact]
    public void Execute_ReturnsWinFormsResult_WhenHostIsWinForms()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new ReturningHostHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("WinForms", result);
    }

    // ── Default fallback behavior ─────────────────────────────────────────────

    [Fact]
    public void Execute_ReturnsBlazorValue_WhenOnWpfNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Wpf };
        var handler = new BlazorOnlyReturningHostHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public void Execute_ReturnsBlazorValue_WhenOnWinFormsNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.WinForms };
        var handler = new BlazorOnlyReturningHostHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
    }

    [Fact]
    public void Execute_ReturnsBlazorValue_WhenOnMauiNotOverridden()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        var handler = new BlazorOnlyReturningHostHandler(bridge);

        var result = handler.Execute();

        Assert.Equal("Blazor", result);
    }

    // ── Unknown host ──────────────────────────────────────────────────────────

    [Fact]
    public void Execute_ThrowsBridgeException_WhenHostIsUnknown()
    {
        var bridge = new FakeBridge { Host = Host.Unknown };
        var handler = new BlazorOnlyReturningHostHandler(bridge);

        var act = () => handler.Execute();

        Assert.Throws<BridgeException>(act);
    }

}
