using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Providers;

public sealed class BridgeFormFactorProviderTests : BunitContext
{
    private readonly FakeBridgeFormFactor _formFactor;

    public BridgeFormFactorProviderTests()
    {
        _formFactor = new FakeBridgeFormFactor();
        Services.AddSingleton<IBridgeFormFactor>(_formFactor);
    }

    [Fact]
    public void ChildContent_IsRendered_AfterInitialization()
    {
        var cut = Render<BridgeFormFactorProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal("content", cut.Find("span").TextContent);
    }

    [Fact]
    public void InitializesFormFactorService()
    {
        Render<BridgeFormFactorProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal(1, _formFactor.InitializeCallCount);
    }

    [Fact]
    public void Mode_IsPassedToFormFactorService()
    {
        Render<BridgeFormFactorProvider>(p => p
            .Add(c => c.Mode, ResizeMode.Global)
            .AddChildContent("<span>content</span>"));

        Assert.Equal(ResizeMode.Global, _formFactor.LastResizeMode);
    }

    [Fact]
    public void Mode_DefaultsToNone()
    {
        Render<BridgeFormFactorProvider>(p => p
            .AddChildContent("<span>content</span>"));

        Assert.Equal(ResizeMode.None, _formFactor.LastResizeMode);
    }
}
