namespace Circuids.Bridge;

/// <summary>
/// Configuration for connectivity monitoring.
/// </summary>
public sealed class ConnectivityOptions
{
    /// <summary>
    /// Polling interval for web-based connectivity checks.
    /// Ignored on MAUI (uses native ConnectivityChanged event).
    /// Default: 10 seconds.
    /// </summary>
    public int IntervalInSeconds { get; set; } = 10;

    /// <summary>
    /// URL to ping for connectivity verification on web.
    /// Default: "/favicon.ico" (self-hosted, avoids external dependencies).
    /// </summary>
    public string TestUrl { get; set; } = "/favicon.ico";
}
