namespace Circuids.Bridge;

/// <summary>
/// Provides safe area insets for notched/cutout devices.
/// </summary>
public interface IBridgeSafeArea
{
    /// <summary>
    /// The current safe area insets.
    /// </summary>
    SafeAreaInsets SafeArea { get; }

    /// <summary>
    /// Fires when safe area insets change (e.g., orientation change).
    /// </summary>
    event EventHandler<SafeAreaInsets>? SafeAreaChanged;

    /// <summary>
    /// Initializes safe area detection.
    /// </summary>
    Task InitializeAsync();
}
