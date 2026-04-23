using Circuids.Bridge.Blazor;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Tests.Blazor;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddBridgeForBlazor_RegistersIBridgeAsScoped()
    {
        var services = new ServiceCollection();
        services.AddBridgeForBlazor();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBridge));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
    }

    [Fact]
    public void AddBridgeForBlazor_RegistersIBridgeFormFactorAsScoped()
    {
        var services = new ServiceCollection();
        services.AddBridgeForBlazor();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBridgeFormFactor));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
    }

    [Fact]
    public void AddBridgeForBlazor_RegistersIBridgeConnectivityAsScoped()
    {
        var services = new ServiceCollection();
        services.AddBridgeForBlazor();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBridgeConnectivity));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
    }

    [Fact]
    public void AddBridgeForBlazor_RegistersIBridgeThemeAsScoped()
    {
        var services = new ServiceCollection();
        services.AddBridgeForBlazor();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBridgeTheme));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
    }

    [Fact]
    public void AddBridgeForBlazor_RegistersIBridgeSafeAreaAsScoped()
    {
        var services = new ServiceCollection();
        services.AddBridgeForBlazor();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBridgeSafeArea));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor!.Lifetime);
    }

    [Fact]
    public void AddBridgeForBlazor_RegistersExactlyFiveServices()
    {
        var services = new ServiceCollection();
        services.AddBridgeForBlazor();

        Assert.Equal(5, services.Count);
    }
}
