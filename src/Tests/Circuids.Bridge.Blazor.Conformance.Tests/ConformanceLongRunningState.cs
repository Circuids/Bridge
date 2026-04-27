namespace Circuids.Bridge.Blazor.Conformance.Tests;

public sealed class ConformanceLongRunningState
{
    public bool IsEnabled { get; set; }

    public TimeSpan ObservationDuration { get; set; } = TimeSpan.FromSeconds(15);
}