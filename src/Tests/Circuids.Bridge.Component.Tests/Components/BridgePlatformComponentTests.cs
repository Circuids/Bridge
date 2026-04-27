using Circuids.Bridge.TestSupport.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Components;

public sealed class BridgePlatformComponentTests : BunitContext
{
    [Theory]
    [InlineData(PlatformIdentity.Android, "android")]
    [InlineData(PlatformIdentity.IOS, "ios")]
    [InlineData(PlatformIdentity.Windows, "windows")]
    [InlineData(PlatformIdentity.Mac, "mac")]
    [InlineData(PlatformIdentity.Linux, "linux")]
    public void BridgePlatform_RendersMatchingPlatformSlot(PlatformIdentity platform, string expected)
    {
        var bridge = new FakeBridge { Platform = platform };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgePlatform>(parameters => parameters
            .Add(component => component.Android, "<span>android</span>")
            .Add(component => component.IOS, "<span>ios</span>")
            .Add(component => component.Windows, "<span>windows</span>")
            .Add(component => component.Mac, "<span>mac</span>")
            .Add(component => component.Linux, "<span>linux</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal(expected, cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgePlatform_RendersChildContentBeforePlatformSlot()
    {
        var bridge = new FakeBridge { Platform = PlatformIdentity.Windows };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgePlatform>(parameters => parameters
            .Add(component => component.Windows, "<span>windows</span>")
            .Add(component => component.ChildContent, (PlatformIdentity platform) => $"<strong>{platform}</strong>"));

        Assert.Equal("Windows", cut.Find("strong").TextContent);
        Assert.Equal("windows", cut.Find("span").TextContent);
        Assert.Equal("strong", cut.Nodes[0].NodeName.ToLowerInvariant());
    }

    [Fact]
    public void BridgePlatform_RendersDefaultSlot_WhenPlatformIsUnknown()
    {
        var bridge = new FakeBridge { Platform = PlatformIdentity.Unknown };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgePlatform>(parameters => parameters
            .Add(component => component.Windows, "<span>windows</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgePlatform_RendersDefaultSlot_WhenMatchingPlatformSlotIsNull()
    {
        var bridge = new FakeBridge { Platform = PlatformIdentity.Windows };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgePlatform>(parameters => parameters
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgePlatform_RerendersOnPlatformChanged()
    {
        var bridge = new FakeBridge { Platform = PlatformIdentity.Windows };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgePlatform>(parameters => parameters
            .Add(component => component.Windows, "<span>windows</span>")
            .Add(component => component.Android, "<span>android</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal("windows", cut.Find("span").TextContent);

        bridge.RaisePlatformChanged(PlatformIdentity.Android);

        Assert.Equal("android", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgePlatform_UnsubscribesFromPlatformChangedOnDispose()
    {
        var bridge = new FakeBridge { Platform = PlatformIdentity.Windows };
        Services.AddSingleton<IBridge>(bridge);

        var cut = Render<BridgePlatform>(parameters => parameters
            .Add(component => component.Windows, "<span>windows</span>")
            .Add(component => component.Android, "<span>android</span>")
            .Add(component => component.Default, "<span>default</span>"));

        cut.Dispose();
        bridge.RaisePlatformChanged(PlatformIdentity.Android);
    }
}
