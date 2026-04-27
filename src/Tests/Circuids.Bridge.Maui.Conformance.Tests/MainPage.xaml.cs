using System.Text.Json;

namespace Circuids.Bridge.Maui.Conformance.Tests;

public partial class MainPage : ContentPage
{
    private readonly ITestExecutor _executor;
    private readonly ConformanceFailureSentinelState _failureSentinel;
    private readonly ConformanceLongRunningState _longRunning;
    private readonly ConformanceObservationStore _observations;

    public MainPage(
        ITestExecutor executor,
        ConformanceFailureSentinelState failureSentinel,
        ConformanceLongRunningState longRunning,
        ConformanceObservationStore observations)
    {
        InitializeComponent();
        _executor = executor;
        _failureSentinel = failureSentinel;
        _longRunning = longRunning;
        _observations = observations;
    }

    private async void OnRunClicked(object? sender, EventArgs e)
    {
        RunButton.IsEnabled = false;
        RunButton.Text = "Running";
        SummaryLabel.Text = string.Empty;
        EnvironmentLabel.Text = string.Empty;
        ResultsView.ItemsSource = null;
        ReportJsonEditor.Text = string.Empty;
        ObservationEditor.Text = string.Empty;

        try
        {
            _failureSentinel.IsEnabled = FailureSentinelCheckBox.IsChecked;
            _longRunning.IsEnabled = LongRunningCheckBox.IsChecked;
            _longRunning.ObservationDuration = TimeSpan.FromSeconds(GetLongRunningSeconds());
            _observations.Clear();

            var report = await _executor.RunAsync();
            SummaryLabel.Text =
                $"{(report.Success ? "PASS" : "FAIL")} - " +
                $"{report.Total} total - {report.Passed} passed - " +
                $"{report.Failed} failed - {report.Skipped} skipped";

            EnvironmentLabel.Text =
                $"Platform: {report.AssignedPlatform}\n" +
                $"Framework: {report.RuntimeEnvironment.FrameworkDescription}\n" +
                $"RID: {report.RuntimeEnvironment.RuntimeIdentifier}\n" +
                $"OS: {report.RuntimeEnvironment.OSDescription}";

            ResultsView.ItemsSource = report.Results.Select(result => new ConformanceResultRow(result)).ToList();
            ReportJsonEditor.Text = JsonSerializer.Serialize(report, PulseJsonContext.Default.TestRunReport);
            ObservationEditor.Text = _observations.FormatSummary();
        }
        finally
        {
            RunButton.IsEnabled = true;
            RunButton.Text = "Re-run";
        }
    }

    private void ThrowButton_Clicked(object? sender, EventArgs e)
    {
        throw new PulseAssertionException("Test");
    }

    private int GetLongRunningSeconds()
    {
        if (int.TryParse(LongRunningSecondsEntry.Text, out var seconds))
            return Math.Clamp(seconds, 5, 60);

        return 15;
    }

}