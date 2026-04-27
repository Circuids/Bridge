# Circuids.Bridge.Blazor

`Circuids.Bridge.Blazor` is the Bridge host package for Blazor WebAssembly and Blazor Server. It registers browser-backed implementations for Bridge's shared services so Razor components can adapt to host, platform, form factor, connectivity, theme, and safe-area state.

Install this package in the Blazor app that runs your UI. Shared Razor Class Libraries can reference `Circuids.Bridge` directly, or rely on this package transitively from the host app.

## Install

```bash
dotnet add package Circuids.Bridge.Blazor
```

## Register Services

### Blazor WebAssembly

```csharp
using Circuids.Bridge.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddBridgeForBlazor();

await builder.Build().RunAsync();
```

### Blazor Server

```csharp
using Circuids.Bridge.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBridgeForBlazor();

var app = builder.Build();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

Bridge initializes after the interactive Blazor runtime is available. In Blazor Server apps that use static prerendering, Bridge components render with default values first and then update after the circuit connects.

## Add The Provider

Wrap the layout or root subtree that should receive initialized Bridge state.

```razor
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

`BridgeProvider` initializes services in this order: host/platform, form factor, connectivity, theme, and safe area.

## Use In Components

```razor
@using Circuids.Bridge

<BridgeFormFactor @bind-FormFactor="currentFormFactor" Context="viewport">
    <span>@viewport.Width x @viewport.Height</span>

    <Phone>
        <MobileDashboard />
    </Phone>
    <Desktop>
        <DesktopDashboard />
    </Desktop>
    <Default>
        <CompactDashboard />
    </Default>
</BridgeFormFactor>

<BridgeConnectivity @bind-IsConnected="isConnected">
    <Online>
        <SyncStatus />
    </Online>
    <Offline>
        <OfflineBanner />
    </Offline>
</BridgeConnectivity>

@code {
    private FormFactorInfo currentFormFactor = FormFactorInfo.Unknown();
    private bool isConnected;
}
```

Plain child content receives the current context. Named slots such as `Phone`, `Desktop`, `Online`, and `Offline` are simple branch fragments.

## Safe Area On Web

For web safe-area insets, add `viewport-fit=cover` to your host page.

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
```

Without that value, browsers report zero safe-area insets.

## Package Relationship

| Package | Use |
|---------|-----|
| `Circuids.Bridge` | Core contracts and Razor components for shared libraries. |
| `Circuids.Bridge.Blazor` | Blazor WebAssembly and Blazor Server host implementation. |
| `Circuids.Bridge.Maui` | MAUI Blazor Hybrid host implementation. |

## Learn More

- Repository: https://github.com/Circuids/Bridge
- Getting started: https://github.com/Circuids/Bridge/blob/main/docs/getting-started.md
- Usage guide: https://github.com/Circuids/Bridge/blob/main/docs/usage.md
