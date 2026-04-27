using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.TestSupport.Contracts;

public sealed class BridgeServiceRegistrationInfo
{
    public BridgeServiceRegistrationInfo(
        Type serviceType,
        int registrationCount,
        ServiceLifetime? lifetime,
        Type? implementationType)
    {
        ServiceType = serviceType;
        RegistrationCount = registrationCount;
        Lifetime = lifetime;
        ImplementationType = implementationType;
    }

    public Type ServiceType { get; }

    public int RegistrationCount { get; }

    public ServiceLifetime? Lifetime { get; }

    public Type? ImplementationType { get; }

    public bool IsScoped => RegistrationCount == 1 && Lifetime == ServiceLifetime.Scoped;

    public string FailureMessage
    {
        get
        {
            if (RegistrationCount == 0)
                return $"{ServiceType.Name} is not registered.";

            if (RegistrationCount > 1)
                return $"{ServiceType.Name} is registered {RegistrationCount} times; expected exactly one registration.";

            if (Lifetime != ServiceLifetime.Scoped)
                return $"{ServiceType.Name} is registered as {Lifetime}; expected Scoped.";

            return string.Empty;
        }
    }
}