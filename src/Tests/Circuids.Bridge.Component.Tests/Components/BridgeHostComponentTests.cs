using Circuids.Bridge.TestSupport.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Components;

public sealed class BridgeHostComponentTests : BunitContext
{
    [Theory]
    [InlineData(Host.Maui, "maui")]
    [InlineData(Host.Blazor, "blazor")]
    [InlineData(Host.Wpf, "wpf")]
    [InlineData(Host.WinForms, "winforms")]
    public void BridgeHost_RendersMatchingHostSlot(Host host, string expected)
    {
        var bridge = new FakeBridge { Host = host };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgeHost>(parameters => parameters
            .Add(component => component.Maui, "<span>maui</span>")
            .Add(component => component.Blazor, "<span>blazor</span>")
            .Add(component => component.Wpf, "<span>wpf</span>")
            .Add(component => component.WinForms, "<span>winforms</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal(expected, cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeHost_RendersChildContentBeforeHostSlot()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgeHost>(parameters => parameters
            .Add(component => component.Maui, "<span id=\"slot\">maui</span>")
            .Add(component => component.ChildContent, (Host host) => $"<strong>{host}</strong>"));

        Assert.Equal("maui", cut.Find("#slot").TextContent);
        Assert.Equal("Maui", cut.Find("strong").TextContent);
        Assert.Equal("strong", cut.Nodes[0].NodeName.ToLowerInvariant());
    }

    [Fact]
    public void BridgeHost_RendersDefaultSlot_ForUnknownHost()
    {
        var bridge = new FakeBridge { Host = Host.Unknown };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgeHost>(parameters => parameters
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeHost_RendersEmptyContent_WhenMatchedHostSlotIsNull()
    {
        var bridge = new FakeBridge { Host = Host.Maui };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgeHost>(parameters => parameters
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Empty(cut.FindAll("span"));
    }
}
