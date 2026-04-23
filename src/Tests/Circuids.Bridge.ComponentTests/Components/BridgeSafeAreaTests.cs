using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Components;

public sealed class BridgeSafeAreaTests : BunitContext
{
    private readonly FakeBridgeSafeArea _safeArea;

    public BridgeSafeAreaTests()
    {
        _safeArea = new FakeBridgeSafeArea();
        Services.AddSingleton<IBridgeSafeArea>(_safeArea);
    }

    [Fact]
    public void Renders_ChildContent_WithCurrentSafeAreaInsets()
    {
        _safeArea.SafeArea = new SafeAreaInsets(44, 0, 34, 0);

        var cut = Render<BridgeSafeArea>(p => p
            .Add(c => c.ChildContent, (SafeAreaInsets s) => $"<span>{s.Top},{s.Bottom}</span>"));

        Assert.Equal("44,34", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_ChildContent_WithZeroInsets_ByDefault()
    {
        _safeArea.SafeArea = SafeAreaInsets.Zero;

        var cut = Render<BridgeSafeArea>(p => p
            .Add(c => c.ChildContent, (SafeAreaInsets s) => $"<span>{s.HasInsets}</span>"));

        Assert.Equal("False", cut.Find("span").TextContent);
    }

    [Fact]
    public void Re_Renders_WhenSafeAreaChanges()
    {
        _safeArea.SafeArea = SafeAreaInsets.Zero;

        var cut = Render<BridgeSafeArea>(p => p
            .Add(c => c.ChildContent, (SafeAreaInsets s) => $"<span>{s.Top}</span>"));

        _safeArea.RaiseSafeAreaChanged(new SafeAreaInsets(44, 0, 34, 0));

        Assert.Equal("44", cut.Find("span").TextContent);
    }

    [Fact]
    public void Unsubscribes_FromSafeAreaChanged_OnDispose()
    {
        _safeArea.SafeArea = SafeAreaInsets.Zero;

        var cut = Render<BridgeSafeArea>(p => p
            .Add(c => c.ChildContent, (SafeAreaInsets _) => "<span>content</span>"));

        cut.Instance.Dispose();

        var act = () => _safeArea.RaiseSafeAreaChanged(new SafeAreaInsets(44, 0, 34, 0));
        act();
    }
}
