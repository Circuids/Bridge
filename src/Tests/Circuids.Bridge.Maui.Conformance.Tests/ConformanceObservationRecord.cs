namespace Circuids.Bridge.Maui.Conformance.Tests;

public sealed class ConformanceObservationRecord
{
    public ConformanceObservationRecord(DateTimeOffset timestamp, string source, string detail)
    {
        Timestamp = timestamp;
        Source = source;
        Detail = detail;
    }

    public DateTimeOffset Timestamp { get; }

    public string Source { get; }

    public string Detail { get; }
}