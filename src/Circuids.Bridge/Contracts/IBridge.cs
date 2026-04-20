namespace Circuids.Bridge;

/// <summary>
/// Core bridge interface for detecting the host environment and platform.
/// </summary>
public interface IBridge
{
    /// <summary>
    /// The host environment: Maui, Blazor, Wpf, WinForms, etc.
    /// </summary>
    Host Host { get; }

    /// <summary>
    /// The detected platform: Android, iOS, Windows, Mac, Linux, Web.
    /// </summary>
    PlatformIdentity Platform { get; }

    /// <summary>
    /// Platform version string. "Unknown" when not detectable.
    /// </summary>
    string PlatformVersion { get; }

    /// <summary>
    /// Whether the bridge has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Fires when the platform is detected (important for pre-rendering scenarios).
    /// </summary>
    event EventHandler<PlatformIdentity>? PlatformChanged;

    /// <summary>
    /// Initializes the bridge. Called automatically by BridgeProvider.
    /// Safe to call multiple times.
    /// </summary>
    Task InitializeAsync();
}
