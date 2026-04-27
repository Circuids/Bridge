using Circuids.Bridge.Component.Tests.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Providers;

public sealed class BridgeSafeAreaProviderComponentTests : BunitContext
{
    [Fact]
    public void BridgeSafeAreaProvider_InitializesServiceAndRendersChildContent()
    {
        var recorder = new InitializationRecorder();
        var safeArea = new RecordingSafeArea(recorder);
        Services.AddSingleton<IBridgeSafeArea>(safeArea);

        var cut = Render<BridgeSafeAreaProvider>(parameters => parameters
            .AddChildContent("<span>safe</span>"));

        Assert.Equal("safe", cut.Find("span").TextContent);
        Assert.Equal(new[] { "SafeArea" }, recorder.Calls);
    }

    [Fact]
    public void BridgeSafeAreaProvider_InitializesServiceOnlyOnceAcrossRerenders()
    {
        var safeArea = new RecordingSafeArea(new InitializationRecorder());
        Services.AddSingleton<IBridgeSafeArea>(safeArea);

        var cut = Render<BridgeSafeAreaProvider>(parameters => parameters
            .AddChildContent("<span>safe</span>"));

        cut.Render();

        Assert.Equal(1, safeArea.InitializeCallCount);
    }
}
