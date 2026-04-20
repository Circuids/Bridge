# Usage Guide

This guide covers all Bridge features with practical examples. All components and interfaces are in the `Circuids.Bridge` namespace.

```razor
@using Circuids.Bridge
```

---

## Table of Contents

1. [Host Detection](#host-detection)
2. [Platform Detection](#platform-detection)
3. [Form Factor Detection](#form-factor-detection)
4. [Connectivity Monitoring](#connectivity-monitoring)
5. [Theme Detection](#theme-detection)
6. [Safe Area Insets](#safe-area-insets)
7. [BridgeHostHandler — Host-Specific Logic in C#](#bridgehosthandler)
8. [Two-Way Binding](#two-way-binding)
9. [Using Services Directly via DI](#using-services-directly-via-di)

---

## Host Detection

Detect whether the app is running in MAUI, Blazor, WPF, or WinForms.

### Component

```razor
<BridgeHost>
    <Maui>
        <p>Running inside MAUI Blazor Hybrid</p>
    </Maui>
    <Blazor>
        <p>Running in the browser (Blazor WASM or Server)</p>
    </Blazor>
    <Default>
        <p>Unknown host environment</p>
    </Default>
</BridgeHost>
```

### With context value

```razor
<BridgeHost>
    @if (context == Host.Maui)
    {
        <MauiSpecificComponent />
    }
    else
    {
        <WebSpecificComponent />
    }
</BridgeHost>
```

### Via DI

```razor
@inject IBridge Bridge

<p>Host: @Bridge.Host</p>
```

---

## Platform Detection

Detect the operating system: Android, iOS, Windows, Mac, Linux, or Web.

### Component

```razor
<BridgePlatform>
    <Android>Android device</Android>
    <IOS>iPhone or iPad</IOS>
    <Windows>Windows desktop</Windows>
    <Mac>macOS</Mac>
    <Linux>Linux</Linux>
    <Web>Generic web browser</Web>
    <Default>Unknown platform</Default>
</BridgePlatform>
```

### Via DI

```razor
@inject IBridge Bridge

<p>Platform: @Bridge.Platform</p>
<p>Version: @Bridge.PlatformVersion</p>
```

### Reacting to changes

The `PlatformChanged` event fires once the platform is detected (useful when the provider initializes after the component renders):

```razor
@inject IBridge Bridge
@implements IDisposable

<p>Platform: @Bridge.Platform</p>

@code {
    protected override void OnInitialized()
    {
        Bridge.PlatformChanged += OnPlatformChanged;
    }

    private void OnPlatformChanged(object? sender, PlatformIdentity e)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose() => Bridge.PlatformChanged -= OnPlatformChanged;
}
```

---

## Form Factor Detection

Classify the device as Phone, Tablet, or Desktop based on viewport width.

| Form Factor | Width Range |
|-------------|------------|
| Phone | ≤ 767px |
| Tablet | 768px – 1023px |
| Desktop | ≥ 1024px |

### Component — named fragments

```razor
<BridgeFormFactor>
    <Phone>
        <MobileLayout />
    </Phone>
    <Tablet>
        <TabletLayout />
    </Tablet>
    <Desktop>
        <DesktopLayout />
    </Desktop>
    <Default>
        <p>Loading...</p>
    </Default>
</BridgeFormFactor>
```

### Combination fragments

Target multiple form factors with a single fragment:

```razor
<BridgeFormFactor>
    <Desktop>
        <SidebarLayout />
    </Desktop>
    <TabletAndPhone>
        <StackedLayout />
    </TabletAndPhone>
</BridgeFormFactor>
```

Available combinations: `DesktopAndTablet`, `DesktopAndPhone`, `TabletAndPhone`.

**Resolution order:**
- Phone: `Phone` → `TabletAndPhone` → `DesktopAndPhone` → `Default`
- Tablet: `Tablet` → `TabletAndPhone` → `DesktopAndTablet` → `Default`
- Desktop: `Desktop` → `DesktopAndTablet` → `DesktopAndPhone` → `Default`

### Context value — access dimensions

```razor
<BridgeFormFactor>
    @{
        var info = context;
    }
    <p>Form factor: @info.FormFactor</p>
    <p>Viewport: @info.Width × @info.Height</p>
</BridgeFormFactor>
```

### Listen once (no resize tracking)

For static layouts that don't need to respond to resizes:

```razor
<BridgeFormFactor ListenOnce="true">
    <Phone><MobileView /></Phone>
    <Desktop><DesktopView /></Desktop>
</BridgeFormFactor>
```

### Via DI

```razor
@inject IBridgeFormFactor FormFactor

<p>Current: @FormFactor.FormFactor.FormFactor (@FormFactor.FormFactor.Width × @FormFactor.FormFactor.Height)</p>
```

---

## Connectivity Monitoring

Monitor internet connectivity in real-time.

### Component

```razor
<BridgeConnectivity>
    <Online>
        <p>You're connected to the internet.</p>
    </Online>
    <Offline>
        <div class="alert alert-warning">
            No internet connection. Some features may be unavailable.
        </div>
    </Offline>
</BridgeConnectivity>
```

### Context value

```razor
<BridgeConnectivity>
    <p>Online: @context</p>
</BridgeConnectivity>
```

### Via DI

```razor
@inject IBridgeConnectivity Connectivity
@implements IDisposable

<p>Connected: @Connectivity.IsConnected</p>

@code {
    protected override void OnInitialized()
    {
        Connectivity.ConnectionChanged += OnConnectionChanged;
    }

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose() => Connectivity.ConnectionChanged -= OnConnectionChanged;
}
```

### Configuring connectivity checks (Blazor only)

On Blazor, connectivity is checked by polling a URL. Configure this via `ConnectivityOptions`:

```razor
<BridgeProvider ConnectivityOptions="@(new ConnectivityOptions
{
    IntervalInSeconds = 30,
    TestUrl = "/api/health"
})">
    @Body
</BridgeProvider>
```

| Property | Default | Description |
|----------|---------|-------------|
| `IntervalInSeconds` | `10` | Polling interval in seconds. |
| `TestUrl` | `"/favicon.ico"` | URL to HEAD-request. Use a self-hosted endpoint to avoid CORS and regional blocks. |

> On MAUI, connectivity uses the native `Connectivity.ConnectivityChanged` event — no polling needed. `ConnectivityOptions` is ignored.

---

## Theme Detection

Detect the system light/dark mode preference.

### Component

```razor
<BridgeTheme>
    <Light>
        <div class="light-theme">Light mode content</div>
    </Light>
    <Dark>
        <div class="dark-theme">Dark mode content</div>
    </Dark>
    <Default>
        <div>Theme not detected</div>
    </Default>
</BridgeTheme>
```

### Context value

```razor
<BridgeTheme>
    <p>Current theme: @context</p>
</BridgeTheme>
```

### Via DI

```razor
@inject IBridgeTheme Theme
@implements IDisposable

<p>Theme: @Theme.Theme</p>

@code {
    protected override void OnInitialized()
    {
        Theme.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, ThemeMode mode)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose() => Theme.ThemeChanged -= OnThemeChanged;
}
```

---

## Safe Area Insets

Get safe area insets for notched/cutout devices (iPhone notch, Android camera cutout, gesture navigation bars).

### Component

```razor
<BridgeSafeArea>
    @{
        var insets = context;
    }
    <div style="padding: @(insets.Top)px @(insets.Right)px @(insets.Bottom)px @(insets.Left)px;">
        <p>Content with safe area padding</p>
    </div>
</BridgeSafeArea>
```

### Via DI

```razor
@inject IBridgeSafeArea SafeArea

@if (SafeArea.SafeArea.HasInsets)
{
    <div style="padding-top: @(SafeArea.SafeArea.Top)px; padding-bottom: @(SafeArea.SafeArea.Bottom)px;">
        @ChildContent
    </div>
}
```

### Blazor HTML requirement

For safe area insets to work in Blazor, add `viewport-fit=cover` to your HTML:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
```

---

## BridgeHostHandler

Execute different C# logic depending on the host — without `#if` preprocessor directives.

### Returning a value

```csharp
using Circuids.Bridge;

public class StoragePathHandler(IBridge bridge) : BridgeHostHandler<string>(bridge)
{
    protected override string OnMaui() => FileSystem.AppDataDirectory;
    protected override string OnBlazor() => "/local-storage";
}

// Usage
var handler = new StoragePathHandler(bridge);
string path = handler.Execute();
```

### Void (side effects)

```csharp
public class NotificationHandler(IBridge bridge) : BridgeHostHandler(bridge)
{
    protected override void OnMaui()
    {
        // Show native MAUI notification
    }

    protected override void OnBlazor()
    {
        // Show browser notification via JS interop
    }
}
```

### Async via Task return

```csharp
public class DataSyncHandler(IBridge bridge) : BridgeHostHandler<Task>(bridge)
{
    protected override async Task OnMaui()
    {
        await SyncWithSqlite();
    }

    protected override async Task OnBlazor()
    {
        await SyncWithIndexedDb();
    }
}

// Usage
await handler.Execute();
```

### Default fallbacks

`OnWpf()` and `OnWinForms()` default to calling `OnBlazor()`. Override them only if you need different behavior:

```csharp
public class ClipboardHandler(IBridge bridge) : BridgeHostHandler(bridge)
{
    protected override void OnMaui() => /* MAUI clipboard */;
    protected override void OnBlazor() => /* JS clipboard API */;
    protected override void OnWpf() => /* WPF-specific clipboard */;
    // OnWinForms() falls back to OnBlazor() by default
}
```

---

## Two-Way Binding

The `BridgeFormFactor` and `BridgeConnectivity` components support two-way binding:

### Form factor

```razor
<BridgeFormFactor @bind-FormFactor="currentFormFactor">
    <Phone>Phone UI</Phone>
    <Desktop>Desktop UI</Desktop>
</BridgeFormFactor>

<p>Current: @currentFormFactor.FormFactor</p>

@code {
    private FormFactorInfo currentFormFactor = FormFactorInfo.Unknown();
}
```

### Connectivity

```razor
<BridgeConnectivity @bind-IsConnected="isOnline">
    <Online>Online</Online>
    <Offline>Offline</Offline>
</BridgeConnectivity>

@code {
    private bool isOnline;
}
```

---

## Using Services Directly via DI

All features are available as injectable services — you don't have to use the Razor components.

```razor
@inject IBridge Bridge
@inject IBridgeFormFactor FormFactor
@inject IBridgeConnectivity Connectivity
@inject IBridgeTheme Theme
@inject IBridgeSafeArea SafeArea
```

| Service | Key Properties/Events |
|---------|-----------------------|
| `IBridge` | `Host`, `Platform`, `PlatformVersion`, `IsInitialized`, `PlatformChanged` |
| `IBridgeFormFactor` | `FormFactor`, `FormFactorChanged`, `CreateListenerAsync()`, `DisposeListenerAsync()` |
| `IBridgeConnectivity` | `IsConnected`, `ConnectionChanged` |
| `IBridgeTheme` | `Theme`, `ThemeChanged` |
| `IBridgeSafeArea` | `SafeArea`, `SafeAreaChanged` |

> **Important:** Services must be initialized before use. The `<BridgeProvider>` (or individual providers) handles this automatically. If you bypass providers, call `InitializeAsync()` manually.
