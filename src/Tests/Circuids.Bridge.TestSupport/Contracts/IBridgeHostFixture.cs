using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.TestSupport.Contracts;

/// <summary>
/// Shared contract describing how a host integration registers Bridge
/// services. Implementations supply the <c>AddBridgeFor*</c> entry point and
/// any additional setup required to compose a service collection.
/// </summary>
public interface IBridgeHostFixture
{
    /// <summary>
    /// Returns a <see cref="IServiceCollection"/> with the host's
    /// <c>AddBridgeFor*</c> extension applied.
    /// </summary>
    IServiceCollection BuildServices();
}
