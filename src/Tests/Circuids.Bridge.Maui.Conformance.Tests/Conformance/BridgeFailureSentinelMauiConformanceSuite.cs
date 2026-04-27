namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeFailureSentinelMauiConformanceSuite
{
    private readonly ConformanceFailureSentinelState _state;

    public BridgeFailureSentinelMauiConformanceSuite(ConformanceFailureSentinelState state)
    {
        _state = state;
    }

    [PulseCase]
    public Task Intentional_failure_sentinel_reports_failure_when_enabled()
    {
        if (!_state.IsEnabled)
        {
            PulseAssert.True(true);
            return Task.CompletedTask;
        }

        PulseAssert.True(false, "Intentional MAUI conformance failure sentinel was enabled.");
        return Task.CompletedTask;
    }
}