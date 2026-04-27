using Circuids.Pulse;

namespace Circuids.Bridge.Blazor.Conformance.Tests.Conformance;

public sealed class BridgeValueObjectsBlazorConformanceSuite
{
    [PulseCase]
    public Task FormFactorInfo_unknown_uses_zero_dimensions()
    {
        var info = FormFactorInfo.Unknown();

        PulseAssert.Equal(FormFactor.Unknown, info.FormFactor);
        PulseAssert.Equal(0d, info.Width);
        PulseAssert.Equal(0d, info.Height);

        return Task.CompletedTask;
    }

    [PulseCase]
    public Task FormFactorInfo_unknown_overload_preserves_dimensions()
    {
        var info = FormFactorInfo.Unknown(320, 640);

        PulseAssert.Equal(FormFactor.Unknown, info.FormFactor);
        PulseAssert.Equal(320d, info.Width);
        PulseAssert.Equal(640d, info.Height);

        return Task.CompletedTask;
    }

    [PulseCase]
    public Task FormFactorInfo_uses_value_equality()
    {
        var first = new FormFactorInfo(FormFactor.Tablet, 900, 1200);
        var second = new FormFactorInfo(FormFactor.Tablet, 900, 1200);

        PulseAssert.Equal(first, second);

        return Task.CompletedTask;
    }

    [PulseCase]
    public Task SafeAreaInsets_zero_has_no_insets()
    {
        PulseAssert.False(SafeAreaInsets.Zero.HasInsets);
        PulseAssert.Equal(new SafeAreaInsets(0, 0, 0, 0), SafeAreaInsets.Zero);

        return Task.CompletedTask;
    }

    [PulseCase]
    public Task SafeAreaInsets_has_insets_when_any_edge_is_positive()
    {
        PulseAssert.True(new SafeAreaInsets(1, 0, 0, 0).HasInsets);
        PulseAssert.True(new SafeAreaInsets(0, 1, 0, 0).HasInsets);
        PulseAssert.True(new SafeAreaInsets(0, 0, 1, 0).HasInsets);
        PulseAssert.True(new SafeAreaInsets(0, 0, 0, 1).HasInsets);
        PulseAssert.False(new SafeAreaInsets(-1, 0, 0, 0).HasInsets);

        return Task.CompletedTask;
    }
}
