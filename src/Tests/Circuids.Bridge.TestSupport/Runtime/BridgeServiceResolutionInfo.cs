namespace Circuids.Bridge.TestSupport.Runtime;

public sealed class BridgeServiceResolutionInfo
{
    public BridgeServiceResolutionInfo(Type serviceType, bool isResolved, string? errorMessage)
    {
        ServiceType = serviceType;
        IsResolved = isResolved;
        ErrorMessage = errorMessage;
    }

    public Type ServiceType { get; }

    public bool IsResolved { get; }

    public string? ErrorMessage { get; }

    public string FailureMessage => ErrorMessage ?? $"{ServiceType.Name} did not resolve from the service provider.";
}