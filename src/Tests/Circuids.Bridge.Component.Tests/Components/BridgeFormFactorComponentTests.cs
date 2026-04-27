using Circuids.Bridge.TestSupport.Fakes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Circuids.Bridge.Component.Tests.Components;

public sealed class BridgeFormFactorComponentTests : BunitContext
{
    private readonly FakeBridgeFormFactor _formFactor;

    public BridgeFormFactorComponentTests()
    {
        _formFactor = new FakeBridgeFormFactor();
        Services.AddSingleton<IBridgeFormFactor>(_formFactor);
    }

    [Theory]
    [InlineData(FormFactor.Phone, "phone")]
    [InlineData(FormFactor.Tablet, "tablet")]
    [InlineData(FormFactor.Desktop, "desktop")]
    public void Renders_PrimarySlot_ForActiveFormFactor(FormFactor formFactor, string expected)
    {
        _formFactor.FormFactor = new FormFactorInfo(formFactor, 800, 600);

        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Phone, "<span>phone</span>")
            .Add(component => component.Tablet, "<span>tablet</span>")
            .Add(component => component.Desktop, "<span>desktop</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal(expected, cut.Find("span").TextContent);
    }

    [Fact]
    public void Phone_FallbackOrder_IsPhone_TabletAndPhone_DesktopAndPhone_Default()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var tabletAndPhone = RenderFormFactor(component => component.TabletAndPhone, "tablet-phone");
        Assert.Equal("tablet-phone", tabletAndPhone.Find("span").TextContent);

        var desktopAndPhone = RenderFormFactor(component => component.DesktopAndPhone, "desktop-phone");
        Assert.Equal("desktop-phone", desktopAndPhone.Find("span").TextContent);

        var fallback = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Default, "<span>default</span>"));
        Assert.Equal("default", fallback.Find("span").TextContent);
    }

    [Fact]
    public void Tablet_FallbackOrder_IsTablet_TabletAndPhone_DesktopAndTablet_Default()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Tablet, 768, 1024);

        var tabletAndPhone = RenderFormFactor(component => component.TabletAndPhone, "tablet-phone");
        Assert.Equal("tablet-phone", tabletAndPhone.Find("span").TextContent);

        var desktopAndTablet = RenderFormFactor(component => component.DesktopAndTablet, "desktop-tablet");
        Assert.Equal("desktop-tablet", desktopAndTablet.Find("span").TextContent);

        var fallback = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Default, "<span>default</span>"));
        Assert.Equal("default", fallback.Find("span").TextContent);
    }

    [Fact]
    public void Desktop_FallbackOrder_IsDesktop_DesktopAndTablet_DesktopAndPhone_Default()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        var desktopAndTablet = RenderFormFactor(component => component.DesktopAndTablet, "desktop-tablet");
        Assert.Equal("desktop-tablet", desktopAndTablet.Find("span").TextContent);

        var desktopAndPhone = RenderFormFactor(component => component.DesktopAndPhone, "desktop-phone");
        Assert.Equal("desktop-phone", desktopAndPhone.Find("span").TextContent);

        var fallback = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Default, "<span>default</span>"));
        Assert.Equal("default", fallback.Find("span").TextContent);
    }

    [Fact]
    public void ChildContent_ReceivesCurrentFormFactor()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.ChildContent, (FormFactorInfo info) => $"<span>{info.FormFactor}</span>"));

        Assert.Equal("Desktop", cut.Find("span").TextContent);
    }

    [Fact]
    public void ChildContent_RendersBeforeSelectedSlot()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.ChildContent, (FormFactorInfo info) => $"<strong>{info.FormFactor}</strong>")
            .Add(component => component.Desktop, "<span>desktop</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal("Desktop", cut.Find("strong").TextContent);
        Assert.Equal("desktop", cut.Find("span").TextContent);
        Assert.Equal("strong", cut.Nodes[0].NodeName.ToLowerInvariant());
    }

    [Fact]
    public void UnknownFormFactor_RendersDefaultSlot()
    {
        _formFactor.FormFactor = FormFactorInfo.Unknown();

        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Phone, "<span>phone</span>")
            .Add(component => component.Tablet, "<span>tablet</span>")
            .Add(component => component.Desktop, "<span>desktop</span>")
            .Add(component => component.Default, "<span>default</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    [Fact]
    public void RendersEmptyContent_WhenNoSlotMatchesAndDefaultIsNull()
    {
        _formFactor.FormFactor = FormFactorInfo.Unknown();

        var cut = Render<BridgeFormFactor>();

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void FormFactor_Rerenders_WhenServiceRaisesChange()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Phone, "<span>phone</span>")
            .Add(component => component.Desktop, "<span>desktop</span>")
            .Add(component => component.Default, "<span>default</span>"));

        _formFactor.RaiseFormFactorChanged(new FormFactorInfo(FormFactor.Desktop, 1920, 1080));

        cut.WaitForAssertion(() => Assert.Equal("desktop", cut.Find("span").TextContent));
    }

    [Fact]
    public void FormFactorChangedCallback_IsInvokedOnInitialRenderAndServiceChange()
    {
        var initial = new FormFactorInfo(FormFactor.Phone, 390, 844);
        var changed = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);
        _formFactor.FormFactor = initial;
        var values = new List<FormFactorInfo>();

        Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Phone, "<span>phone</span>")
            .Add(component => component.Desktop, "<span>desktop</span>")
            .Add(component => component.Default, "<span>default</span>")
            .Add(component => component.FormFactorChanged, values.Add));

        _formFactor.RaiseFormFactorChanged(changed);

        Assert.Equal(new[] { initial, changed }, values);
    }

    [Fact]
    public void ListenOnce_DoesNotSubscribeToFormFactorChanged()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.ListenOnce, true)
            .Add(component => component.Phone, "<span>phone</span>")
            .Add(component => component.Desktop, "<span>desktop</span>")
            .Add(component => component.Default, "<span>default</span>"));

        _formFactor.RaiseFormFactorChanged(new FormFactorInfo(FormFactor.Desktop, 1920, 1080));

        Assert.Equal("phone", cut.Find("span").TextContent);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    public void ListenOnce_ControlsListenerCreation(bool listenOnce, int expectedCreateListenerCalls)
    {
        Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.ListenOnce, listenOnce));

        Assert.Equal(expectedCreateListenerCalls, _formFactor.CreateListenerCallCount);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    public async Task DisposeAsync_DisposesListenerOnlyWhenListening(bool listenOnce, int expectedDisposeListenerCalls)
    {
        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.ListenOnce, listenOnce)
            .Add(component => component.Default, "<span>default</span>"));

        await cut.Instance.DisposeAsync();

        Assert.Equal(expectedDisposeListenerCalls, _formFactor.DisposeListenerCallCount);
    }

    [Fact]
    public async Task DisposeAsync_UnsubscribesFromFormFactorChanged()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var cut = Render<BridgeFormFactor>(parameters => parameters
            .Add(component => component.Phone, "<span>phone</span>")
            .Add(component => component.Desktop, "<span>desktop</span>")
            .Add(component => component.Default, "<span>default</span>"));

        await cut.Instance.DisposeAsync();
        _formFactor.RaiseFormFactorChanged(new FormFactorInfo(FormFactor.Desktop, 1920, 1080));
    }

    private IRenderedComponent<BridgeFormFactor> RenderFormFactor(
        Expression<Func<BridgeFormFactor, RenderFragment?>> slotSelector,
        string text)
    {
        return Render<BridgeFormFactor>(parameters => parameters
            .Add(slotSelector, $"<span>{text}</span>")
            .Add(component => component.Default, "<span>default</span>"));
    }
}