using System.Globalization;
using System.Text;

namespace Circuids.Bridge.Maui.Conformance.Tests;

public sealed class ConformanceObservationStore
{
    private readonly object _sync = new();
    private readonly List<ConformanceObservationRecord> _records = new();

    public void Clear()
    {
        lock (_sync)
        {
            _records.Clear();
        }
    }

    public void Add(string source, string detail)
    {
        lock (_sync)
        {
            _records.Add(new ConformanceObservationRecord(DateTimeOffset.UtcNow, source, detail));
        }
    }

    public string FormatSummary()
    {
        List<ConformanceObservationRecord> records;

        lock (_sync)
        {
            records = _records.ToList();
        }

        if (records.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        foreach (var record in records)
        {
            builder
                .Append(record.Timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture))
                .Append(" [")
                .Append(record.Source)
                .Append("] ")
                .AppendLine(record.Detail);
        }

        return builder.ToString();
    }
}