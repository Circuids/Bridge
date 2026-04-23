using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Components;

public sealed class BridgePlatformTests : BunitContext
{
    private readonly FakeBridge _bridge;

    public BridgePlatformTests()
    {
        _bridge = new FakeBridge();
        Services.AddSingleton<IBridge>(_bridge);
    }

    [Fact]
    public void Renders_WindowsSlot_WhenPlatformIsWindows()
    {
        _bridge.Platform = PlatformIdentity.Windows;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.Windows, "<span>windows</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("windows", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_AndroidSlot_WhenPlatformIsAndroid()
    {
        _bridge.Platform = PlatformIdentity.Android;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.Android, "<span>android</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("android", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_IOSSlot_WhenPlatformIsIOS()
    {
        _bridge.Platform = PlatformIdentity.IOS;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.IOS, "<span>ios</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("ios", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_MacSlot_WhenPlatformIsMac()
    {
        _bridge.Platform = PlatformIdentity.Mac;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.Mac, "<span>mac</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("mac", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_LinuxSlot_WhenPlatformIsLinux()
    {
        _bridge.Platform = PlatformIdentity.Linux;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.Linux, "<span>linux</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("linux", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_DefaultSlot_WhenNoPlatformSlotMatches()
    {
        _bridge.Platform = PlatformIdentity.Windows;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_DefaultSlot_WhenPlatformIsUnknown()
    {
        _bridge.Platform = PlatformIdentity.Unknown;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.Default, "<span>default</span>")
            .Add(c => c.Windows, "<span>windows</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_ChildContent_WithCurrentPlatform()
    {
        _bridge.Platform = PlatformIdentity.Windows;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.ChildContent, (PlatformIdentity p) => $"<span>{p}</span>"));

        Assert.Equal("Windows", cut.Find("span").TextContent);
    }

    [Fact]
    public void Re_Renders_WhenPlatformChanges()
    {
        _bridge.Platform = PlatformIdentity.Windows;

        var cut = Render<BridgePlatform>(p => p
            .Add(c => c.Windows, "<span>windows</span>")
            .Add(c => c.Android, "<span>android</span>")
            .Add(c => c.Default, "<span>default</span>"));

        _bridge.RaisePlatformChanged(PlatformIdentity.Android);

        Assert.Equal("android", cut.Find("span").TextContent);
    }
}
