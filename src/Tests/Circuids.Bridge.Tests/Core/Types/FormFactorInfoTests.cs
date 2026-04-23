namespace Circuids.Bridge.Tests.Core.Types;

public sealed class FormFactorInfoTests
{
    [Fact]
    public void Unknown_ReturnsFormFactorUnknown()
    {
        var info = FormFactorInfo.Unknown();

        Assert.Equal(FormFactor.Unknown, info.FormFactor);
        Assert.Equal(0, info.Width);
        Assert.Equal(0, info.Height);
    }

    [Fact]
    public void Unknown_WithDimensions_RetainsWidthAndHeight()
    {
        var info = FormFactorInfo.Unknown(1024, 768);

        Assert.Equal(FormFactor.Unknown, info.FormFactor);
        Assert.Equal(1024, info.Width);
        Assert.Equal(768, info.Height);
    }

    [Fact]
    public void Constructor_StoresAllValues()
    {
        var info = new FormFactorInfo(FormFactor.Desktop, 1920, 1080);

        Assert.Equal(FormFactor.Desktop, info.FormFactor);
        Assert.Equal(1920, info.Width);
        Assert.Equal(1080, info.Height);
    }

    [Fact]
    public void TwoInstances_WithSameValues_AreEqual()
    {
        var a = new FormFactorInfo(FormFactor.Phone, 390, 844);
        var b = new FormFactorInfo(FormFactor.Phone, 390, 844);

        Assert.Equal(b, a);
    }

    [Fact]
    public void TwoInstances_WithDifferentValues_AreNotEqual()
    {
        var a = new FormFactorInfo(FormFactor.Phone, 390, 844);
        var b = new FormFactorInfo(FormFactor.Tablet, 390, 844);

        Assert.NotEqual(b, a);
    }
}
