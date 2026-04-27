using Circuids.Bridge.Component.Tests.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Component.Tests.Providers;

public sealed class BridgeFormFactorProviderComponentTests : BunitContext
{
    [Fact]
    public void BridgeFormFactorProvider_InitializesServiceAndRendersChildContent()
    {
        var recorder = new InitializationRecorder();
        var formFactor = new RecordingFormFactor(recorder);
        Services.AddSingleton<IBridgeFormFactor>(formFactor);

        var cut = Render<BridgeFormFactorProvider>(parameters => parameters
            .Add(component => component.Mode, ResizeMode.Once)
            .AddChildContent("<span>form</span>"));

        Assert.Equal("form", cut.Find("span").TextContent);
        Assert.Equal(ResizeMode.Once, formFactor.LastResizeMode);
        Assert.Equal(new[] { "FormFactor" }, recorder.Calls);
    }

    [Fact]
    public void BridgeFormFactorProvider_UsesDefaultMode()
    {
        var formFactor = new RecordingFormFactor(new InitializationRecorder());
        Services.AddSingleton<IBridgeFormFactor>(formFactor);

        Render<BridgeFormFactorProvider>(parameters => parameters
            .AddChildContent("<span>form</span>"));

        Assert.Equal(ResizeMode.None, formFactor.LastResizeMode);
    }

    [Fact]
    public void BridgeFormFactorProvider_InitializesServiceOnlyOnceAcrossRerenders()
    {
        var formFactor = new RecordingFormFactor(new InitializationRecorder());
        Services.AddSingleton<IBridgeFormFactor>(formFactor);

        var cut = Render<BridgeFormFactorProvider>(parameters => parameters
            .AddChildContent("<span>form</span>"));

        cut.Render();

        Assert.Equal(1, formFactor.InitializeCallCount);
    }
}
