using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Providers;

public sealed class BridgeThemeProviderTests : BunitContext
{
    private readonly FakeBridgeTheme _theme;

    public BridgeThemeProviderTests()
    {
        _theme = new FakeBridgeTheme();
        Services.AddSingleton<IBridgeTheme>(_theme);
    }

    [Fact]
    public void ChildContent_IsRendered_AfterInitialization()
    {
        var cut = Render<BridgeThemeProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal("content", cut.Find("span").TextContent);
    }

    [Fact]
    public void InitializesThemeService()
    {
        Render<BridgeThemeProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal(1, _theme.InitializeCallCount);
    }
}
