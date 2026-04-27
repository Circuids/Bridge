using Circuids.Bridge.Component.Tests.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Providers;

public sealed class BridgeThemeProviderComponentTests : BunitContext
{
    [Fact]
    public void BridgeThemeProvider_InitializesServiceAndRendersChildContent()
    {
        var recorder = new InitializationRecorder();
        var theme = new RecordingTheme(recorder);
        Services.AddSingleton<IBridgeTheme>(theme);

        var cut = Render<BridgeThemeProvider>(parameters => parameters
            .AddChildContent("<span>theme</span>"));

        Assert.Equal("theme", cut.Find("span").TextContent);
        Assert.Equal(new[] { "Theme" }, recorder.Calls);
    }

    [Fact]
    public void BridgeThemeProvider_InitializesServiceOnlyOnceAcrossRerenders()
    {
        var theme = new RecordingTheme(new InitializationRecorder());
        Services.AddSingleton<IBridgeTheme>(theme);

        var cut = Render<BridgeThemeProvider>(parameters => parameters
            .AddChildContent("<span>theme</span>"));

        cut.Render();

        Assert.Equal(1, theme.InitializeCallCount);
    }
}
