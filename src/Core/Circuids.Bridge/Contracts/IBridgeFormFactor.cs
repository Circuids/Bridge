namespace Circuids.Bridge;

/// <summary>
/// Provides form factor detection and resize listening capabilities.
/// </summary>
public interface IBridgeFormFactor
{
    /// <summary>
    /// The current form factor and viewport dimensions.
    /// </summary>
    FormFactorInfo FormFactor { get; }

    /// <summary>
    /// Fires when the form factor changes (e.g., window resize crosses a breakpoint).
    /// </summary>
    event EventHandler<FormFactorInfo>? FormFactorChanged;

    /// <summary>
    /// Initializes the form factor service.
    /// </summary>
    /// <param name="resizeMode">Controls how resize events are handled.</param>
    Task InitializeAsync(ResizeMode resizeMode = ResizeMode.None);

    /// <summary>
    /// Creates a resize listener. Used internally by components.
    /// </summary>
    Task CreateListenerAsync();

    /// <summary>
    /// Disposes the resize listener. Used internally by components.
    /// </summary>
    ValueTask DisposeListenerAsync();
}
