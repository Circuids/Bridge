using Circuids.Bridge.TestSupport.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Components;

public sealed class BridgeThemeComponentTests : BunitContext
{
    [Theory]
    [InlineData(ThemeMode.Light, "light")]
    [InlineData(ThemeMode.Dark, "dark")]
    [InlineData(ThemeMode.Unknown, "default")]
    public void BridgeTheme_RendersCurrentTheme(ThemeMode mode, string expected)
    {
        var theme = new FakeBridgeTheme { Theme = mode };
        Services.AddSingleton<IBridgeTheme>(theme);

        var cut = Render<BridgeTheme>(parameters => parameters
            .Add(component => component.Light, "<span>light</span>")
            .Add(component => component.Dark, "<span>dark</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal(expected, cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeTheme_RendersChildContentBeforeThemeSlot()
    {
        var theme = new FakeBridgeTheme { Theme = ThemeMode.Light };
        Services.AddSingleton<IBridgeTheme>(theme);

        var cut = Render<BridgeTheme>(parameters => parameters
            .Add(component => component.Light, "<span>light</span>")
            .Add(component => component.Dark, "<span>dark</span>")
            .Add(component => component.Default, "<span>default</span>")
            .Add(component => component.ChildContent, (ThemeMode mode) => $"<strong>{mode}</strong>"));

        Assert.Equal("Light", cut.Find("strong").TextContent);
        Assert.Equal("light", cut.Find("span").TextContent);
        Assert.Equal("strong", cut.Nodes[0].NodeName.ToLowerInvariant());
    }

    [Fact]
    public void BridgeTheme_RerendersOnThemeChanged()
    {
        var theme = new FakeBridgeTheme { Theme = ThemeMode.Light };
        Services.AddSingleton<IBridgeTheme>(theme);

        var cut = Render<BridgeTheme>(parameters => parameters
            .Add(component => component.Light, "<span>light</span>")
            .Add(component => component.Dark, "<span>dark</span>")
            .Add(component => component.Default, "<span>default</span>"));

        theme.RaiseThemeChanged(ThemeMode.Dark);

        Assert.Equal("dark", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeTheme_UnsubscribesFromThemeChangedOnDispose()
    {
        var theme = new FakeBridgeTheme { Theme = ThemeMode.Light };
        Services.AddSingleton<IBridgeTheme>(theme);

        var cut = Render<BridgeTheme>(parameters => parameters
            .Add(component => component.Light, "<span>light</span>")
            .Add(component => component.Dark, "<span>dark</span>")
            .Add(component => component.Default, "<span>default</span>"));

        cut.Dispose();
        theme.RaiseThemeChanged(ThemeMode.Dark);
    }
}
