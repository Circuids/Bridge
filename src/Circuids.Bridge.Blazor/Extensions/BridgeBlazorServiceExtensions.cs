using Circuids.Bridge.Blazor.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Blazor;

public static class BridgeBlazorServiceExtensions
{
    /// <summary>
    /// Registers all Bridge services for Blazor WASM/Server.
    /// Use with <c>&lt;BridgeProvider&gt;</c> in the render tree.
    /// </summary>
    public static IServiceCollection AddBridgeForBlazor(this IServiceCollection services)
    {
        services.AddScoped<IBridge, BridgeBlazor>();
        services.AddScoped<IBridgeFormFactor, BridgeFormFactorBlazor>();
        services.AddScoped<IBridgeConnectivity, BridgeConnectivityBlazor>();
        services.AddScoped<IBridgeTheme, BridgeThemeBlazor>();
        services.AddScoped<IBridgeSafeArea, BridgeSafeAreaBlazor>();

        return services;
    }
}
