using Circuids.Bridge.TestSupport.Contracts;
using Circuids.Bridge.TestSupport.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeServiceResolutionMauiConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeServiceResolutionMauiConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task Required_bridge_services_resolve_from_real_host_container()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var results = BridgeServiceResolutionProbe.ProbeRequiredServices(scope.ServiceProvider);

        PulseAssert.Equal(BridgeServiceRegistrationInspector.RequiredServiceTypes.Count, results.Count);

        foreach (var result in results)
        {
            PulseAssert.True(result.IsResolved, result.FailureMessage);
        }
    }

    [PulseCase]
    public async Task Required_bridge_services_are_scoped_within_real_host_scope()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        foreach (var serviceType in BridgeServiceRegistrationInspector.RequiredServiceTypes)
        {
            var firstService = scope.ServiceProvider.GetRequiredService(serviceType);
            var secondService = scope.ServiceProvider.GetRequiredService(serviceType);

            PulseAssert.True(ReferenceEquals(firstService, secondService), $"{serviceType.Name} should reuse the scoped instance inside one scope.");
        }
    }

    [PulseCase]
    public async Task Required_bridge_services_are_isolated_across_real_host_scopes()
    {
        await using var firstScope = _serviceScopeFactory.CreateAsyncScope();
        await using var secondScope = _serviceScopeFactory.CreateAsyncScope();

        foreach (var serviceType in BridgeServiceRegistrationInspector.RequiredServiceTypes)
        {
            var firstService = firstScope.ServiceProvider.GetRequiredService(serviceType);
            var secondService = secondScope.ServiceProvider.GetRequiredService(serviceType);

            PulseAssert.False(ReferenceEquals(firstService, secondService), $"{serviceType.Name} should not leak scoped instances across scopes.");
        }
    }
}