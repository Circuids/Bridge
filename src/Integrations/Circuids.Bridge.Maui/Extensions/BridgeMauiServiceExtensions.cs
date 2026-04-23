using Circuids.Bridge.Maui.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui;

public static class BridgeMauiServiceExtensions
{
    /// <summary>
    /// Registers all Bridge services for MAUI Blazor Hybrid.
    /// Use with <c>&lt;BridgeProvider&gt;</c> in the render tree.
    /// </summary>
    public static IServiceCollection AddBridgeForMaui(this IServiceCollection services)
    {
        services.AddScoped<IBridge, BridgeMaui>();
        services.AddScoped<IBridgeFormFactor, BridgeFormFactorMaui>();
        services.AddScoped<IBridgeConnectivity, BridgeConnectivityMaui>();
        services.AddScoped<IBridgeTheme, BridgeThemeMaui>();
        services.AddScoped<IBridgeSafeArea, BridgeSafeAreaMaui>();

        return services;
    }
}
