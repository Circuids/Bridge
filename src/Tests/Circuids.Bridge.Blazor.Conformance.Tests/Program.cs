using Circuids.Bridge.Blazor;
using Circuids.Bridge.Blazor.Conformance.Tests;
using Circuids.Bridge.Blazor.Conformance.Tests.Conformance;
using Circuids.Pulse.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<ConformanceFailureSentinelState>();
builder.Services.AddSingleton<ConformanceLongRunningState>();
builder.Services.AddSingleton<ConformanceObservationStore>();
builder.Services.AddBridgeForBlazor();
builder.Services.AddPulse(pulse =>
{
    pulse.AssignedPlatform = "Bridge.Blazor.WebAssembly";
    pulse.DefaultTestTimeout = TimeSpan.FromSeconds(10);
    pulse.AddSuite<BridgeFailureSentinelBlazorConformanceSuite>();
    pulse.AddSuite<BridgeLongRunningBlazorConformanceSuite>();
    pulse.AddSuite<BridgeServiceResolutionBlazorConformanceSuite>();
    pulse.AddSuite<BridgeConnectivityOptionsBlazorConformanceSuite>();
    pulse.AddSuite<BridgeValueObjectsBlazorConformanceSuite>();
    pulse.AddSuite<BridgeHostHandlersBlazorConformanceSuite>();
    pulse.AddSuite<BridgeBlazorConformanceSuite>();
    pulse.AddSuite<BridgeConnectivityBlazorConformanceSuite>();
    pulse.AddSuite<BridgeFormFactorBlazorConformanceSuite>();
    pulse.AddSuite<BridgeThemeBlazorConformanceSuite>();
    pulse.AddSuite<BridgeSafeAreaBlazorConformanceSuite>();
});

await builder.Build().RunAsync();