namespace Circuids.Bridge;

/// <summary>
/// Controls how the form factor resize listener behaves.
/// </summary>
public enum ResizeMode
{
    /// <summary>
    /// No listener attached at initialization. Components manage their own listeners.
    /// </summary>
    None,

    /// <summary>
    /// A single persistent listener shared across all components.
    /// </summary>
    Global,

    /// <summary>
    /// Read the form factor once at initialization. No ongoing listening.
    /// </summary>
    Once,
}
