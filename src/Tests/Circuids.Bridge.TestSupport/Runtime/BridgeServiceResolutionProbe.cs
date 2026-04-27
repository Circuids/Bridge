using Circuids.Bridge.TestSupport.Contracts;

namespace Circuids.Bridge.TestSupport.Runtime;

public static class BridgeServiceResolutionProbe
{
    public static IReadOnlyList<BridgeServiceResolutionInfo> ProbeRequiredServices(IServiceProvider serviceProvider)
    {
        var results = new List<BridgeServiceResolutionInfo>(BridgeServiceRegistrationInspector.RequiredServiceTypes.Count);

        foreach (var serviceType in BridgeServiceRegistrationInspector.RequiredServiceTypes)
        {
            try
            {
                var service = serviceProvider.GetService(serviceType);
                results.Add(new BridgeServiceResolutionInfo(serviceType, service is not null, null));
            }
            catch (Exception exception)
            {
                results.Add(new BridgeServiceResolutionInfo(serviceType, false, exception.Message));
            }
        }

        return results;
    }
}