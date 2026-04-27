# Circuids.Bridge

Bridge helps shared Razor UI understand the host it is running in without scattering platform checks through your components. It gives Blazor WebAssembly, Blazor Server, and MAUI Blazor Hybrid apps one common model for host, platform, form factor, connectivity, theme, and safe-area state.

Use the core package when you are building shared Razor UI. It contains the public contracts, enums, value objects, providers, components, and host handlers. Host apps should install one of the implementation packages listed below.

## Choose The Right Package

| Project type | Install | Why |
|--------------|---------|-----|
| Shared Razor Class Library | `Circuids.Bridge` | Compile against Bridge components and interfaces without choosing a host runtime. |
| Blazor WebAssembly app | `Circuids.Bridge.Blazor` | Registers browser-backed implementations using JS interop. |
| Blazor Server app | `Circuids.Bridge.Blazor` | Registers server-compatible Blazor implementations that initialize after the circuit is interactive. |
| MAUI Blazor Hybrid app | `Circuids.Bridge.Maui` | Registers native MAUI implementations for device, window, connectivity, theme, and safe-area state. |

`Circuids.Bridge.Blazor` and `Circuids.Bridge.Maui` both reference this core package transitively. You normally install `Circuids.Bridge` directly only in shared Razor Class Libraries.

## What Bridge Provides

| Need | Component | Service |
|------|-----------|---------|
| Host-specific rendering | `BridgeHost` | `IBridge` |
| Platform-specific rendering | `BridgePlatform` | `IBridge` |
| Phone, tablet, or desktop layout | `BridgeFormFactor` | `IBridgeFormFactor` |
| Online/offline UI | `BridgeConnectivity` | `IBridgeConnectivity` |
| Light/dark UI | `BridgeTheme` | `IBridgeTheme` |
| Notch, cutout, and gesture-safe padding | `BridgeSafeArea` | `IBridgeSafeArea` |

## Shared UI Example

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

<BridgeFormFactor Context="viewport">
    <DashboardFrame Layout="viewport.FormFactor" Width="viewport.Width" Height="viewport.Height" />

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
```

The plain child content receives the current context. Named slots such as `Phone`, `Desktop`, `Maui`, and `Blazor` stay simple branch fragments.

## Host Setup

Install the host package in the application that runs your UI, then wrap the shared UI in `BridgeProvider`.

```razor
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

See the host package README for registration details:

| Host | Package |
|------|---------|
| Blazor WebAssembly or Blazor Server | `Circuids.Bridge.Blazor` |
| MAUI Blazor Hybrid | `Circuids.Bridge.Maui` |

## Learn More

- Repository: https://github.com/Circuids/Bridge
- Getting started: https://github.com/Circuids/Bridge/blob/main/docs/getting-started.md
- Usage guide: https://github.com/Circuids/Bridge/blob/main/docs/usage.md
- API reference: https://github.com/Circuids/Bridge/blob/main/docs/api-reference.md
