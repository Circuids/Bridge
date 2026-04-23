using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Components;

public sealed class BridgeHostTests : BunitContext
{
    private readonly FakeBridge _bridge;

    public BridgeHostTests()
    {
        _bridge = new FakeBridge();
        Services.AddSingleton<IBridge>(_bridge);
    }

    [Fact]
    public void Renders_BlazorSlot_WhenHostIsBlazor()
    {
        _bridge.Host = Host.Blazor;

        var cut = Render<BridgeHost>(p => p
            .Add(c => c.Blazor, "<span>blazor</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("blazor", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_MauiSlot_WhenHostIsMaui()
    {
        _bridge.Host = Host.Maui;

        var cut = Render<BridgeHost>(p => p
            .Add(c => c.Maui, "<span>maui</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("maui", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_WpfSlot_WhenHostIsWpf()
    {
        _bridge.Host = Host.Wpf;

        var cut = Render<BridgeHost>(p => p
            .Add(c => c.Wpf, "<span>wpf</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("wpf", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_WinFormsSlot_WhenHostIsWinForms()
    {
        _bridge.Host = Host.WinForms;

        var cut = Render<BridgeHost>(p => p
            .Add(c => c.WinForms, "<span>winforms</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("winforms", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_DefaultSlot_WhenHostIsUnknown()
    {
        _bridge.Host = Host.Unknown;

        var cut = Render<BridgeHost>(p => p
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_EmptyContent_WhenMatchingSlotIsNull()
    {
        _bridge.Host = Host.Maui;

        // BridgeHost renders null RenderFragment (empty) when the matched slot parameter is null
        var cut = Render<BridgeHost>(p => p
            .Add(c => c.Default, "<span>should-not-appear</span>"));

        Assert.Empty(cut.FindAll("span"));
    }

    [Fact]
    public void Renders_ChildContent_WithCurrentHost()
    {
        _bridge.Host = Host.Blazor;

        var cut = Render<BridgeHost>(p => p
            .Add(c => c.ChildContent, (Host h) => $"<span>{h}</span>"));

        Assert.Equal("Blazor", cut.Find("span").TextContent);
    }
}
