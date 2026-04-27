using Circuids.Bridge.Maui;
using Circuids.Bridge.Maui.Conformance.Tests.Conformance;
using Circuids.Pulse.Extensions;
using Microsoft.Extensions.Logging;

namespace Circuids.Bridge.Maui.Conformance.Tests;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<ConformanceFailureSentinelState>();
        builder.Services.AddSingleton<ConformanceLongRunningState>();
        builder.Services.AddSingleton<ConformanceObservationStore>();
        builder.Services.AddBridgeForMaui();
        builder.Services.AddPulse(pulse =>
        {
            pulse.AssignedPlatform = "Bridge.Maui";
            pulse.DefaultTestTimeout = TimeSpan.FromSeconds(10);
            pulse.AddSuite<BridgeFailureSentinelMauiConformanceSuite>();
            pulse.AddSuite<BridgeLongRunningMauiConformanceSuite>();
            pulse.AddSuite<BridgeServiceResolutionMauiConformanceSuite>();
            pulse.AddSuite<BridgeConnectivityOptionsMauiConformanceSuite>();
            pulse.AddSuite<BridgeValueObjectsMauiConformanceSuite>();
            pulse.AddSuite<BridgeHostHandlersMauiConformanceSuite>();
            pulse.AddSuite<BridgeMauiConformanceSuite>();
            pulse.AddSuite<BridgeConnectivityMauiConformanceSuite>();
            pulse.AddSuite<BridgeFormFactorMauiConformanceSuite>();
            pulse.AddSuite<BridgeThemeMauiConformanceSuite>();
            pulse.AddSuite<BridgeSafeAreaMauiConformanceSuite>();
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}