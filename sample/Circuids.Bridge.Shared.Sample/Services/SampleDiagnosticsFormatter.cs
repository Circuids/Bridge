namespace Circuids.Bridge.Shared.Sample.Services;

public sealed class SampleDiagnosticsFormatter
{
    public string DescribePlatform(IBridge bridge) =>
        bridge.PlatformVersion is { Length: > 0 } version && version != "Unknown"
            ? $"{bridge.Platform} ({version})"
            : bridge.Platform.ToString();

    public string DescribeFormFactor(FormFactorInfo info) =>
        $"{info.FormFactor} at {info.Width:0} x {info.Height:0}";

    public string DescribeConnectivity(bool isConnected) =>
        isConnected ? "Online and actively listening for changes." : "Offline. Bridge will raise an event when connectivity returns.";

    public string DescribeTheme(ThemeMode mode) => mode switch
    {
        ThemeMode.Dark => "Dark mode is active.",
        ThemeMode.Light => "Light mode is active.",
        _ => "Theme has not been resolved yet."
    };

    public string DescribeSafeArea(SafeAreaInsets insets) => insets.HasInsets
        ? $"Top {insets.Top:0}, Right {insets.Right:0}, Bottom {insets.Bottom:0}, Left {insets.Left:0}"
        : "No safe-area offsets are currently reported.";
}