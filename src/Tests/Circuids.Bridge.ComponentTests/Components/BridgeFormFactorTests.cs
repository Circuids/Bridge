using Circuids.Bridge.ComponentTests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.ComponentTests.Components;

public sealed class BridgeFormFactorTests : BunitContext
{
    private readonly FakeBridgeFormFactor _formFactor;

    public BridgeFormFactorTests()
    {
        _formFactor = new FakeBridgeFormFactor();
        Services.AddSingleton<IBridgeFormFactor>(_formFactor);
    }

    // ── Slot rendering ────────────────────────────────────────────────────────

    [Fact]
    public void Renders_DesktopSlot_WhenFormFactorIsDesktop()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.Desktop, "<span>desktop</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("desktop", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_TabletSlot_WhenFormFactorIsTablet()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Tablet, 768, 1024);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.Tablet, "<span>tablet</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("tablet", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_PhoneSlot_WhenFormFactorIsPhone()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.Phone, "<span>phone</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("phone", cut.Find("span").TextContent);
    }

    [Fact]
    public void Renders_DefaultSlot_WhenFormFactorIsUnknown()
    {
        _formFactor.FormFactor = FormFactorInfo.Unknown();

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("default", cut.Find("span").TextContent);
    }

    // ── Fallback precedence ───────────────────────────────────────────────────

    [Fact]
    public void Phone_FallsBackTo_TabletAndPhone_WhenPhoneSlotIsNull()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.TabletAndPhone, "<span>tablet-phone</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("tablet-phone", cut.Find("span").TextContent);
    }

    [Fact]
    public void Phone_FallsBackTo_DesktopAndPhone_WhenPhoneAndTabletAndPhoneAreNull()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.DesktopAndPhone, "<span>desktop-phone</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("desktop-phone", cut.Find("span").TextContent);
    }

    [Fact]
    public void Tablet_FallsBackTo_TabletAndPhone_WhenTabletSlotIsNull()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Tablet, 768, 1024);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.TabletAndPhone, "<span>tablet-phone</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("tablet-phone", cut.Find("span").TextContent);
    }

    [Fact]
    public void Tablet_FallsBackTo_DesktopAndTablet_WhenTabletAndTabletAndPhoneAreNull()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Tablet, 768, 1024);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.DesktopAndTablet, "<span>desktop-tablet</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("desktop-tablet", cut.Find("span").TextContent);
    }

    [Fact]
    public void Desktop_FallsBackTo_DesktopAndTablet_WhenDesktopSlotIsNull()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.DesktopAndTablet, "<span>desktop-tablet</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("desktop-tablet", cut.Find("span").TextContent);
    }

    [Fact]
    public void Desktop_FallsBackTo_DesktopAndPhone_WhenDesktopAndDesktopAndTabletAreNull()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.DesktopAndPhone, "<span>desktop-phone</span>")
            .Add(c => c.Default, "<span>default</span>"));

        Assert.Equal("desktop-phone", cut.Find("span").TextContent);
    }

    // ── ChildContent ──────────────────────────────────────────────────────────

    [Fact]
    public void Renders_ChildContent_WithCurrentFormFactor()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.ChildContent, (FormFactorInfo f) => $"<span>{f.FormFactor}</span>"));

        Assert.Equal("Desktop", cut.Find("span").TextContent);
    }

    // ── Event re-render ───────────────────────────────────────────────────────

    [Fact]
    public void Re_Renders_WhenFormFactorChanges()
    {
        _formFactor.FormFactor = new FormFactorInfo(FormFactor.Phone, 390, 844);

        var cut = Render<BridgeFormFactor>(p => p
            .Add(c => c.Phone, "<span>phone</span>")
            .Add(c => c.Desktop, "<span>desktop</span>")
            .Add(c => c.Default, "<span>default</span>"));

        _formFactor.RaiseFormFactorChanged(new FormFactorInfo(FormFactor.Desktop, 1920, 1080));

        Assert.Equal("desktop", cut.Find("span").TextContent);
    }

    // ── CreateListener ────────────────────────────────────────────────────────

    [Fact]
    public void CreateListenerAsync_IsCalled_WhenListenOnceIsFalse()
    {
        _formFactor.FormFactor = FormFactorInfo.Unknown();

        Render<BridgeFormFactor>(p => p
            .Add(c => c.ListenOnce, false));

        Assert.Equal(1, _formFactor.CreateListenerCallCount);
    }

    [Fact]
    public void CreateListenerAsync_IsNotCalled_WhenListenOnceIsTrue()
    {
        _formFactor.FormFactor = FormFactorInfo.Unknown();

        Render<BridgeFormFactor>(p => p
            .Add(c => c.ListenOnce, true));

        Assert.Equal(0, _formFactor.CreateListenerCallCount);
    }
}
