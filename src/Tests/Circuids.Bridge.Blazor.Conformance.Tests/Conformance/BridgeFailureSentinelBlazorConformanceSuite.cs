using Circuids.Pulse;

namespace Circuids.Bridge.Blazor.Conformance.Tests.Conformance;

public sealed class BridgeFailureSentinelBlazorConformanceSuite
{
    private readonly ConformanceFailureSentinelState _state;

    public BridgeFailureSentinelBlazorConformanceSuite(ConformanceFailureSentinelState state)
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

        PulseAssert.True(false, "Intentional Blazor conformance failure sentinel was enabled.");
        return Task.CompletedTask;
    }
}