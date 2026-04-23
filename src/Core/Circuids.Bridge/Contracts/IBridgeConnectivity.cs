namespace Circuids.Bridge;

/// <summary>
/// Provides internet connectivity detection and monitoring.
/// </summary>
public interface IBridgeConnectivity
{
    /// <summary>
    /// Whether the device currently has internet connectivity.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Fires when connectivity status changes.
    /// </summary>
    event EventHandler<bool>? ConnectionChanged;

    /// <summary>
    /// Initializes connectivity monitoring.
    /// </summary>
    /// <param name="options">Configuration for connectivity checks. Uses defaults if null.</param>
    Task InitializeAsync(ConnectivityOptions? options = null);
}
