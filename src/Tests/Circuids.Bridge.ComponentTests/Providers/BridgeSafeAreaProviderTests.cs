using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Providers;

public sealed class BridgeSafeAreaProviderTests : BunitContext
{
    private readonly FakeBridgeSafeArea _safeArea;

    public BridgeSafeAreaProviderTests()
    {
        _safeArea = new FakeBridgeSafeArea();
        Services.AddSingleton<IBridgeSafeArea>(_safeArea);
    }

    [Fact]
    public void ChildContent_IsRendered_AfterInitialization()
    {
        var cut = Render<BridgeSafeAreaProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal("content", cut.Find("span").TextContent);
    }

    [Fact]
    public void InitializesSafeAreaService()
    {
        Render<BridgeSafeAreaProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal(1, _safeArea.InitializeCallCount);
    }
}
