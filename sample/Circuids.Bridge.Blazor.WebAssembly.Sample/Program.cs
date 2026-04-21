using Circuids.Bridge.Blazor;
using Circuids.Bridge.Blazor.WebAssembly.Sample;
using Circuids.Bridge.Shared.Sample;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBridgeForBlazor();
builder.Services.AddBridgeSharedSample();

await builder.Build().RunAsync();
