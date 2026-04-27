# Circuids.Bridge.Maui

`Circuids.Bridge.Maui` is the Bridge host package for MAUI Blazor Hybrid apps. It registers native MAUI implementations for Bridge's shared services so the same Razor components can adapt to host, platform, form factor, connectivity, theme, and safe-area state.

Install this package in the MAUI app that hosts your BlazorWebView. Shared Razor Class Libraries can reference `Circuids.Bridge` directly, or rely on this package transitively from the host app.

## Install

```bash
dotnet add package Circuids.Bridge.Maui
```

## Register Services

Register Bridge beside the MAUI BlazorWebView services.

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

## Add The Provider

Wrap the shared layout or root Blazor subtree that should receive initialized Bridge state.

```razor
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

`BridgeProvider` initializes services in this order: host/platform, form factor, connectivity, theme, and safe area.

## Use In Shared Razor UI

```razor
@using Circuids.Bridge

<BridgeHost>
    <Maui>
        <NativeToolbar />
    </Maui>
    <Blazor>
        <WebToolbar />
    </Blazor>
</BridgeHost>

<BridgeSafeArea Context="safeArea">
    <main style="padding: @(safeArea.Top)px @(safeArea.Right)px @(safeArea.Bottom)px @(safeArea.Left)px">
        <Router AppAssembly="typeof(App).Assembly" />
    </main>
</BridgeSafeArea>
```

`BridgeSafeArea`, `BridgeFormFactor`, `BridgeConnectivity`, `BridgeTheme`, `BridgeHost`, and `BridgePlatform` use the same component API as Blazor. The MAUI package supplies native values from MAUI APIs.

## Native Behavior

| Feature | MAUI source |
|---------|-------------|
| Host and platform | `DeviceInfo` |
| Form factor and viewport | Current MAUI window dimensions |
| Connectivity | `Connectivity.Current` |
| Theme | Application requested theme |
| Safe area | Platform window insets where available |

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
