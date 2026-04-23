namespace Circuids.Bridge;

/// <summary>
/// Exception thrown by Bridge services when configuration or initialization errors occur.
/// </summary>
public sealed class BridgeException : Exception
{
    public BridgeException(string message) : base(message) { }

    public BridgeException(string message, Exception innerException) : base(message, innerException) { }
}
