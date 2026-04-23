using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Components;

public sealed class BridgeThemeTests : BunitContext
{
    private readonly FakeBridgeTheme _theme;

    public BridgeThemeTests()
    {
        _theme = new FakeBridgeTheme();
        Services.AddSingleton<IBridgeTheme>(_theme);
    }

    [Fact]
    public void Renders_LightSlot_WhenThemeIsLight()
    {
        _theme.Theme = ThemeMode.Light;

        var cut = Render<BridgeTheme>(p => p
            .Add(c => c.Light, "<span>light</span>")
            .Add(c => c.Dark, "<span>dark</span>"));

        Assert.Equal("light", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_DarkSlot_WhenThemeIsDark()
    {
        _theme.Theme = ThemeMode.Dark;

        var cut = Render<BridgeTheme>(p => p
            .Add(c => c.Dark, "<span>dark</span>")
            .Add(c => c.Light, "<span>light</span>"));

        Assert.Equal("dark", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_DefaultSlot_WhenThemeIsUnknown()
    {
        _theme.Theme = ThemeMode.Unknown;

        var cut = Render<BridgeTheme>(p => p
            .Add(c => c.Default, "<span>default</span>")
            .Add(c => c.Light, "<span>light</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_ChildContent_WithCurrentTheme()
    {
        _theme.Theme = ThemeMode.Dark;

        var cut = Render<BridgeTheme>(p => p
            .Add(c => c.ChildContent, (ThemeMode t) => $"<span>{t}</span>"));

        Assert.Equal("Dark", cut.Find("span").TextContent);
    }

    [Fact]
    public void Re_Renders_WhenThemeChanges()
    {
        _theme.Theme = ThemeMode.Light;

        var cut = Render<BridgeTheme>(p => p
            .Add(c => c.Light, "<span>light</span>")
            .Add(c => c.Dark, "<span>dark</span>"));

        _theme.RaiseThemeChanged(ThemeMode.Dark);

        Assert.Equal("dark", cut.Find("span").TextContent);
    }

    [Fact]
    public void Unsubscribes_FromThemeChanged_OnDispose()
    {
        _theme.Theme = ThemeMode.Light;

        var cut = Render<BridgeTheme>(p => p
            .Add(c => c.Light, "<span>light</span>")
            .Add(c => c.Dark, "<span>dark</span>"));

        cut.Instance.Dispose();

        // Should not throw and should not re-render after dispose
        var act = () => _theme.RaiseThemeChanged(ThemeMode.Dark);
        act();
    }
}
