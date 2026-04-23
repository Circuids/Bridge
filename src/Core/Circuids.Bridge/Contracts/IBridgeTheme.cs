namespace Circuids.Bridge;

/// <summary>
/// Provides system theme (light/dark mode) detection.
/// </summary>
public interface IBridgeTheme
{
    /// <summary>
    /// The current system theme.
    /// </summary>
    ThemeMode Theme { get; }

    /// <summary>
    /// Fires when the system theme changes.
    /// </summary>
    event EventHandler<ThemeMode>? ThemeChanged;

    /// <summary>
    /// Initializes theme detection.
    /// </summary>
    Task InitializeAsync();
}
