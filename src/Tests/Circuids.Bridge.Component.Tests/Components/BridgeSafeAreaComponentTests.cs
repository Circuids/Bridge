using Circuids.Bridge.TestSupport.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Components;

public sealed class BridgeSafeAreaComponentTests : BunitContext
{
    [Fact]
    public void BridgeSafeArea_RendersCurrentInsets()
    {
        var safeArea = new FakeBridgeSafeArea { SafeArea = new SafeAreaInsets(44, 1, 34, 2) };
        Services.AddSingleton<IBridgeSafeArea>(safeArea);

        var cut = Render<BridgeSafeArea>(parameters => parameters
            .Add(component => component.ChildContent, (SafeAreaInsets insets) => $"<span>{insets.Top},{insets.Right},{insets.Bottom},{insets.Left}</span>"));

        Assert.Equal("44,1,34,2", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeSafeArea_RerendersOnSafeAreaChanged()
    {
        var safeArea = new FakeBridgeSafeArea { SafeArea = SafeAreaInsets.Zero };
        Services.AddSingleton<IBridgeSafeArea>(safeArea);

        var cut = Render<BridgeSafeArea>(parameters => parameters
            .Add(component => component.ChildContent, (SafeAreaInsets insets) => $"<span>{insets.Top}</span>"));

        Assert.Equal("0", cut.Find("span").TextContent);

        safeArea.RaiseSafeAreaChanged(new SafeAreaInsets(44, 0, 34, 0));

        Assert.Equal("44", cut.Find("span").TextContent);
    }

    [Fact]
    public void BridgeSafeArea_RendersEmptyContent_WhenChildContentIsNull()
    {
        Services.AddSingleton<IBridgeSafeArea>(new FakeBridgeSafeArea());

        var cut = Render<BridgeSafeArea>();

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void BridgeSafeArea_UnsubscribesFromSafeAreaChangedOnDispose()
    {
        var safeArea = new FakeBridgeSafeArea { SafeArea = SafeAreaInsets.Zero };
        Services.AddSingleton<IBridgeSafeArea>(safeArea);

        var cut = Render<BridgeSafeArea>(parameters => parameters
            .Add(component => component.ChildContent, (SafeAreaInsets insets) => $"<span>{insets.Top}</span>"));

        cut.Dispose();
        safeArea.RaiseSafeAreaChanged(new SafeAreaInsets(44, 0, 34, 0));
    }
}
