using Circuids.Bridge.Shared.Sample.Navigation;

namespace Circuids.Bridge.Shared.Sample.Services;

public sealed class SampleScenarioRegistry
{
    public IReadOnlyList<SampleScenario> Scenarios { get; } =
    [
        new("Overview", "", "Shared sample entry point and catalog overview.", "Foundation"),
        new("Host Detection", "host-detection", "Render host-specific content and inspect the detected host.", "Host"),
        new("Platform Detection", "platform-detection", "Inspect the operating system reported by Bridge.", "Platform"),
        new("Form Factor", "form-factor", "Classify viewport size and observe resize behavior.", "Viewport"),
        new("Connectivity", "connectivity", "Show online and offline behavior through Bridge connectivity monitoring.", "Network"),
        new("Theme", "theme", "React to light and dark mode changes.", "Theme"),
        new("Safe Area", "safe-area", "Visualize safe area insets for notched and edge-to-edge layouts.", "Insets"),
        new("Host Handlers", "host-handlers", "Exercise the sync and async host-handler APIs.", "Handlers"),
        new("Services", "services", "Use raw Bridge services directly via DI.", "Services"),
        new("Diagnostics", "diagnostics", "Inspect all live runtime values in one place.", "Diagnostics")
    ];
}