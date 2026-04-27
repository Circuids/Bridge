using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.TestSupport.Contracts;

public static class BridgeServiceRegistrationInspector
{
    private static readonly Type[] RequiredServiceTypesInternal =
    {
        typeof(IBridge),
        typeof(IBridgeFormFactor),
        typeof(IBridgeConnectivity),
        typeof(IBridgeTheme),
        typeof(IBridgeSafeArea),
    };

    public static IReadOnlyList<Type> RequiredServiceTypes { get; } = Array.AsReadOnly(RequiredServiceTypesInternal);

    public static IReadOnlyList<BridgeServiceRegistrationInfo> InspectRequiredServices(IServiceCollection services)
    {
        var results = new List<BridgeServiceRegistrationInfo>(RequiredServiceTypesInternal.Length);

        foreach (var serviceType in RequiredServiceTypesInternal)
        {
            var descriptors = services.Where(service => service.ServiceType == serviceType).ToList();
            var descriptor = descriptors.Count == 1 ? descriptors[0] : null;

            results.Add(new BridgeServiceRegistrationInfo(
                serviceType,
                descriptors.Count,
                descriptor?.Lifetime,
                descriptor?.ImplementationType));
        }

        return results;
    }
}