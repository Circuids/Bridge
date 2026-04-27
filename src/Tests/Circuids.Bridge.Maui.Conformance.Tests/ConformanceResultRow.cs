namespace Circuids.Bridge.Maui.Conformance.Tests;

public sealed class ConformanceResultRow
{
    public ConformanceResultRow(TestResult result)
    {
        Outcome = result.Outcome.ToString().ToUpperInvariant();
        TestName = $"{ShortSuite(result.SuiteName)} - {result.TestName}";
        DurationText = $"{result.Duration.TotalMilliseconds:0} ms";
        Message = result.Message ?? string.Empty;
        HasMessage = !string.IsNullOrEmpty(result.Message);
    }

    public string Outcome { get; }

    public string TestName { get; }

    public string DurationText { get; }

    public string Message { get; }

    public bool HasMessage { get; }

    private static string ShortSuite(string suiteName)
    {
        var index = suiteName.LastIndexOf('.');
        return index >= 0 ? suiteName[(index + 1)..] : suiteName;
    }
}
