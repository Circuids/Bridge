<div align="center">
  <img src="https://github.com/Circuids/Bridge/blob/master/images/cover_logo_min.jpg?raw=true" alt="Bridge logo" height="250" width="1000" />
</div>

<div align="center">

[![NuGet](https://img.shields.io/nuget/v/Circuids.Bridge.svg)](https://nuget.org/packages/Circuids.Bridge/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Circuids.Bridge.svg)](https://nuget.org/packages/Circuids.Bridge/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

</div>

# Bridge

Bridge helps shared Razor UI adapt to the host it is running in. Use the same components and services in Blazor WebAssembly, Blazor Server, and MAUI Blazor Hybrid to answer practical runtime questions:

- Am I running on the web or inside MAUI?
- Is this Android, iOS, Windows, Mac, or Linux?
- Should this layout behave like phone, tablet, or desktop?
- Is the app online?
- Is the user in light or dark mode?
- Does the screen need safe-area padding for a notch, cutout, or gesture area?

Bridge keeps those answers behind a small set of components and injectable services, so your shared UI does not need platform `#if` blocks.

## Choose A Package

Install the host package in the app that runs your UI:

| App type | Install |
|----------|---------|
| Blazor WebAssembly | `Circuids.Bridge.Blazor` |
| Blazor Server | `Circuids.Bridge.Blazor` |
| MAUI Blazor Hybrid | `Circuids.Bridge.Maui` |
| Shared Razor Class Library | `Circuids.Bridge` |

The Blazor and MAUI packages reference the core package transitively. Shared Razor Class Libraries normally reference only `Circuids.Bridge`; the host app provides the real implementation through DI.

## Get Started

Bridge setup has two parts: register the host implementation, then put a provider around the UI that should receive initialized Bridge state.

### Blazor WebAssembly

```bash
dotnet add package Circuids.Bridge.Blazor
```

```csharp
using Circuids.Bridge.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddBridgeForBlazor();

await builder.Build().RunAsync();
```

```razor
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

### Blazor Server

```bash
dotnet add package Circuids.Bridge.Blazor
```

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

```razor
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

Bridge initializes after the interactive circuit is available. During static prerendering, components render with default values and update after the circuit connects.

### MAUI Blazor Hybrid

```bash
dotnet add package Circuids.Bridge.Maui
```

```csharp
using Circuids.Bridge.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddBridgeForMaui();

        return builder.Build();
    }
}
```

```razor
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

## Use It In Shared UI

Start with the components when the answer changes what you render.

### Host-Specific UI

```razor
<BridgeHost>
    <Maui>
        <NativeToolbar />
    </Maui>
    <Blazor>
        <WebToolbar />
    </Blazor>
    <Default>
        <StandardToolbar />
    </Default>
</BridgeHost>
```

### Responsive Layout

```razor
<BridgeFormFactor Context="viewport">
    <DashboardFrame Layout="viewport.FormFactor" Width="viewport.Width" Height="viewport.Height" />

    <Phone>
        <MobileDashboard />
    </Phone>
    <TabletAndPhone>
        <CompactDashboard />
    </TabletAndPhone>
    <Desktop>
        <DesktopDashboard />
    </Desktop>
    <Default>
        <LoadingLayout />
    </Default>
</BridgeFormFactor>
```

Plain child content receives the current component context. Named slots such as `Phone`, `Desktop`, `Online`, and `Dark` stay simple branch fragments and are used only for selecting what to render.

`BridgeFormFactor` uses this fallback order:

| Active form factor | First matching slot wins |
|--------------------|--------------------------|
| Phone | `Phone`, `TabletAndPhone`, `DesktopAndPhone`, `Default` |
| Tablet | `Tablet`, `TabletAndPhone`, `DesktopAndTablet`, `Default` |
| Desktop | `Desktop`, `DesktopAndTablet`, `DesktopAndPhone`, `Default` |
| Unknown | `Default` |

### Connectivity State

```razor
<BridgeConnectivity>
    <Online>
        <SyncStatus />
    </Online>
    <Offline>
        <OfflineBanner />
    </Offline>
</BridgeConnectivity>
```

### Theme And Safe Area

```razor
<BridgeTheme>
    <Light><AppShell Theme="light" /></Light>
    <Dark><AppShell Theme="dark" /></Dark>
    <Default><AppShell Theme="system" /></Default>
</BridgeTheme>

<BridgeSafeArea Context="safeArea">
    <div style="padding: @(safeArea.Top)px @(safeArea.Right)px @(safeArea.Bottom)px @(safeArea.Left)px">
        <MainShell />
    </div>
</BridgeSafeArea>
```

For web safe-area insets, add `viewport-fit=cover` to the app host page:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
```

### Bind To Component State

`BridgeFormFactor` and `BridgeConnectivity` can push their current state into fields with normal Blazor binding.

```razor
<BridgeFormFactor @bind-FormFactor="currentFormFactor">
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

<p>@currentFormFactor.FormFactor</p>
<p>@(isConnected ? "Online" : "Offline")</p>

@code {
    private FormFactorInfo currentFormFactor = FormFactorInfo.Unknown();
    private bool isConnected;
}
```

## Use It In C#

Inject services when runtime state belongs in code rather than markup.

```razor
@inject IBridge Bridge
@inject IBridgeFormFactor FormFactor
@inject IBridgeConnectivity Connectivity

<p>@Bridge.Host on @Bridge.Platform</p>
<p>@FormFactor.FormFactor.FormFactor</p>
<p>@(Connectivity.IsConnected ? "Online" : "Offline")</p>
```

For host-specific C# behavior, use a host handler. `OnBlazor()` is the required baseline; MAUI, WPF, and WinForms fall back to it unless you override them.

```csharp
using Circuids.Bridge;

public sealed class StoragePathHandler : BridgeHostHandler<string>
{
    public StoragePathHandler(IBridge bridge) : base(bridge)
    {
    }

    protected override string OnBlazor() => "/local-storage";

    protected override string OnMaui() => FileSystem.AppDataDirectory;
}
```

## Configure The Provider

Most apps can use the default provider:

```razor
<BridgeProvider>
    @Body
</BridgeProvider>
```

Tune behavior only when the app needs it.

```razor
<BridgeProvider
    FormFactorResizeMode="ResizeMode.Global"
    ConnectivityOptions='@(new ConnectivityOptions { IntervalInSeconds = 30, TestUrl = "/health" })'>
    @Body
</BridgeProvider>
```

| Option | When to use it |
|--------|----------------|
| `ResizeMode.None` | Components create form-factor listeners only when needed. This is the default. |
| `ResizeMode.Global` | The app always needs responsive state and should keep one listener active. |
| `ResizeMode.Once` | The app needs the initial form factor only. |
| `ConnectivityOptions` | Blazor apps should poll a self-hosted URL such as `/health` or `/favicon.ico`. MAUI uses native connectivity and ignores this option. |

You can also initialize only one feature with an individual provider:

```razor
<BridgeThemeProvider>
    @Body
</BridgeThemeProvider>
```

Individual providers do not initialize `IBridge`. Use `BridgeProvider` when host/platform state is part of the same subtree.

## What Bridge Provides

| Need | Component | Service |
|------|-----------|---------|
| Host-specific rendering | `BridgeHost` | `IBridge` |
| Platform-specific rendering | `BridgePlatform` | `IBridge` |
| Phone/tablet/desktop layout | `BridgeFormFactor` | `IBridgeFormFactor` |
| Online/offline UI | `BridgeConnectivity` | `IBridgeConnectivity` |
| Light/dark UI | `BridgeTheme` | `IBridgeTheme` |
| Notch/cutout padding | `BridgeSafeArea` | `IBridgeSafeArea` |

## Learn More

- [Getting Started](docs/getting-started.md) - setup details for each host.
- [Usage Guide](docs/usage.md) - complete component and service examples.
- [API Reference](docs/api-reference.md) - interfaces, enums, records, components, providers, and handlers.

## Contributing

Contributions are welcome. Please open an issue or submit a pull request with a clear description of the change and the behavior it affects. For significant changes, please discuss the approach in an issue before implementing it and also add or update tests as needed.

When contributing, please follow the existing code style and patterns. For new features, include unit tests for any new logic and consider adding component tests for any new rendering behavior. If the change affects public runtime behavior, add Pulse conformance cases to ensure it works correctly in real host apps.

- Architecture: [docs/architecture.md](docs/architecture.md)
- Testing: [docs/testing-architecture.md](docs/testing-architecture.md)

## Background

Bridge is the production successor to [MauiBlazorBridge](https://github.com/AathifMahir/MauiBlazorBridge), an earlier experimental package by the same author. That package is now archived. Bridge was written from the ground up as a stable, production-ready implementation of the same concept.

## License

Bridge is licensed under the [MIT License](https://github.com/Circuids/Bridge/blob/main/LICENSE).

## Sponsoring

If Bridge helps your work, you can support ongoing maintenance through [GitHub Sponsors](https://github.com/sponsors/Circuids).
