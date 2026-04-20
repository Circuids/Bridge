<div align="center">
  <img src="images/cover_logo_min.png" alt="Bridge Banner" width="600"/>
</div>

<div align="center">

[![NuGet](https://img.shields.io/nuget/v/Circuids.Bridge.svg)](https://nuget.org/packages/Circuids.Bridge/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Circuids.Bridge.svg)](https://nuget.org/packages/Circuids.Bridge/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

</div>

# Bridge

**Circuids Bridge** detects host environments, form factors, connectivity, themes, and safe areas across Blazor and MAUI Blazor Hybrid -- from a single shared codebase.

**Write once, adapt everywhere** -- Clean component-based APIs, injectable services, and zero platform-specific `#if` directives in your shared UI code.

## Packages

| Package | Description |
|---------|-------------|
| `Circuids.Bridge` | Core -- interfaces, components, enums. Install in shared Razor Class Libraries. |
| `Circuids.Bridge.Blazor` | Blazor WASM and Blazor Server apps (JS interop implementations). |
| `Circuids.Bridge.Maui` | MAUI Blazor Hybrid apps (native platform implementations). |

> `Circuids.Bridge.Blazor` and `Circuids.Bridge.Maui` both reference `Circuids.Bridge` transitively -- you don't need to install the core package separately in host projects.

## Features

- **Host Detection** - Detect whether the app is running in MAUI, Blazor, WPF, or WinForms
- **Platform Detection** - Identify the operating system: Android, iOS, Windows, Mac, Linux, or Web
- **Form Factor Detection** - Classify the device as Phone, Tablet, or Desktop based on viewport width
- **Connectivity Monitoring** - Monitor internet connectivity in real-time
- **Theme Detection** - Detect the system light/dark mode preference
- **Safe Area Insets** - Get safe area insets for notched/cutout devices
- **BridgeHostHandler** - Execute host-specific C# logic without preprocessor directives
- **Two-Way Binding** - Bind form factor and connectivity state directly to your components

---

## Table of Contents

- [Getting Started](#getting-started)
  - [Blazor WebAssembly](#blazor-webassembly)
  - [Blazor Server](#blazor-server)
  - [MAUI Blazor Hybrid](#maui-blazor-hybrid)
  - [Shared Razor Class Library](#shared-razor-class-library-rcl)
- [Usage](#usage)
  - [Host Detection](#host-detection)
  - [Platform Detection](#platform-detection)
  - [Form Factor Detection](#form-factor-detection)
  - [Connectivity Monitoring](#connectivity-monitoring)
  - [Theme Detection](#theme-detection)
  - [Safe Area Insets](#safe-area-insets)
  - [BridgeHostHandler](#bridgehosthandler)
  - [Two-Way Binding](#two-way-binding)
  - [Using Services Directly via DI](#using-services-directly-via-di)
- [Provider Configuration](#provider-configuration)
- [API Reference](#api-reference)
  - [Interfaces](#interfaces)
  - [Enums](#enums)
  - [Records and Models](#records-and-models)
  - [Components](#components)
  - [Providers](#providers)
  - [Handlers](#handlers)
  - [Exceptions](#exceptions)
  - [Extension Methods](#extension-methods)
- [License](#license)
- [Contributing](#contributing)
- [Sponsoring](#sponsoring)

---

# Getting Started

All components and interfaces are in the `Circuids.Bridge` namespace.

```razor
@using Circuids.Bridge
```

## Blazor WebAssembly

### 1. Install the package

```
dotnet add package Circuids.Bridge.Blazor
```

### 2. Register services

```csharp
// Program.cs
using Circuids.Bridge.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddBridgeForBlazor();

await builder.Build().RunAsync();
```

### 3. Add the provider

Wrap your root layout (or `Router`) with `<BridgeProvider>`:

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

That's it -- all five services (`IBridge`, `IBridgeFormFactor`, `IBridgeConnectivity`, `IBridgeTheme`, `IBridgeSafeArea`) are now available via DI.

---

## Blazor Server

### 1. Install the package

```
dotnet add package Circuids.Bridge.Blazor
```

### 2. Register services

```csharp
// Program.cs
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

### 3. Add the provider

```razor
@* MainLayout.razor or Routes.razor *@
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

> **Note:** Bridge services require an interactive render mode. During static SSR pre-rendering, the provider won't initialize until the circuit connects. This is by design -- components will render with default values, then update once the provider initializes.

---

## MAUI Blazor Hybrid

### 1. Install the package

```
dotnet add package Circuids.Bridge.Maui
```

### 2. Register services

```csharp
// MauiProgram.cs
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

### 3. Add the provider

In your Blazor root layout (shared or MAUI-specific):

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase
@using Circuids.Bridge

<BridgeProvider>
    @Body
</BridgeProvider>
```

---

## Shared Razor Class Library (RCL)

If you share UI between Blazor and MAUI, your RCL only needs the **core** package:

```
dotnet add package Circuids.Bridge
```

```razor
@* In your shared RCL *@
@using Circuids.Bridge
@inject IBridge Bridge

<BridgeHost>
    <Maui>Running in MAUI</Maui>
    <Blazor>Running in Blazor</Blazor>
</BridgeHost>
```

The host project (Blazor or MAUI) provides the actual implementations via DI. Your shared library stays host-agnostic.

---

# Usage

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
| Phone | <= 767px |
| Tablet | 768px -- 1023px |
| Desktop | >= 1024px |

### Component -- named fragments

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
- Phone: `Phone` -> `TabletAndPhone` -> `DesktopAndPhone` -> `Default`
- Tablet: `Tablet` -> `TabletAndPhone` -> `DesktopAndTablet` -> `Default`
- Desktop: `Desktop` -> `DesktopAndTablet` -> `DesktopAndPhone` -> `Default`

### Context value -- access dimensions

```razor
<BridgeFormFactor>
    @{
        var info = context;
    }
    <p>Form factor: @info.FormFactor</p>
    <p>Viewport: @info.Width x @info.Height</p>
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

<p>Current: @FormFactor.FormFactor.FormFactor (@FormFactor.FormFactor.Width x @FormFactor.FormFactor.Height)</p>
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

> On MAUI, connectivity uses the native `Connectivity.ConnectivityChanged` event -- no polling needed. `ConnectivityOptions` is ignored.

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

Execute different C# logic depending on the host -- without `#if` preprocessor directives.

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

The `BridgeFormFactor` and `BridgeConnectivity` components support two-way binding.

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

All features are available as injectable services -- you don't have to use the Razor components.

```razor
@inject IBridge Bridge
@inject IBridgeFormFactor FormFactor
@inject IBridgeConnectivity Connectivity
@inject IBridgeTheme Theme
@inject IBridgeSafeArea SafeArea
```

| Service | Key Properties / Events |
|---------|------------------------|
| `IBridge` | `Host`, `Platform`, `PlatformVersion`, `IsInitialized`, `PlatformChanged` |
| `IBridgeFormFactor` | `FormFactor`, `FormFactorChanged`, `CreateListenerAsync()`, `DisposeListenerAsync()` |
| `IBridgeConnectivity` | `IsConnected`, `ConnectionChanged` |
| `IBridgeTheme` | `Theme`, `ThemeChanged` |
| `IBridgeSafeArea` | `SafeArea`, `SafeAreaChanged` |

---

# Provider Configuration

### Composite provider (default -- all features enabled)

```razor
<BridgeProvider>
    @Body
</BridgeProvider>
```

### Form factor resize mode

```razor
<BridgeProvider FormFactorResizeMode="ResizeMode.Global">
    @Body
</BridgeProvider>
```

| Mode | Behavior |
|------|----------|
| `ResizeMode.None` | No global listener. Components create/dispose their own listeners on demand. |
| `ResizeMode.Global` | A single persistent listener shared across all components. Best for apps that always need responsive layout. |
| `ResizeMode.Once` | Reads the form factor once at initialization. No ongoing listening. Lightest option. |

### Connectivity options

```razor
<BridgeProvider ConnectivityOptions="@(new ConnectivityOptions { IntervalInSeconds = 30, TestUrl = "/health" })">
    @Body
</BridgeProvider>
```

### Selective -- individual providers

For maximum control, use individual providers instead of the composite. Place them in any long-lived layout element -- no nesting required:

```razor
@* MainLayout.razor *@
<BridgeFormFactorProvider Mode="ResizeMode.Global">
    <TopBar />
</BridgeFormFactorProvider>

<BridgeThemeProvider>
    @Body
</BridgeThemeProvider>

<BridgeConnectivityProvider>
    <StatusBar />
</BridgeConnectivityProvider>
```

> **Note:** When using individual providers, call `IBridge.InitializeAsync()` yourself if you need host/platform detection. The individual providers don't initialize `IBridge` automatically.

---

# API Reference

## Interfaces

### IBridge

Core bridge service for host and platform detection.

```csharp
public interface IBridge
{
    Host Host { get; }
    PlatformIdentity Platform { get; }
    string PlatformVersion { get; }
    bool IsInitialized { get; }
    event EventHandler<PlatformIdentity>? PlatformChanged;
    Task InitializeAsync();
}
```

### IBridgeFormFactor

Form factor detection with optional resize listening.

```csharp
public interface IBridgeFormFactor
{
    FormFactorInfo FormFactor { get; }
    event EventHandler<FormFactorInfo>? FormFactorChanged;
    Task InitializeAsync(ResizeMode resizeMode = ResizeMode.None);
    Task CreateListenerAsync();
    ValueTask DisposeListenerAsync();
}
```

### IBridgeConnectivity

Internet connectivity monitoring.

```csharp
public interface IBridgeConnectivity
{
    bool IsConnected { get; }
    event EventHandler<bool>? ConnectionChanged;
    Task InitializeAsync(ConnectivityOptions? options = null);
}
```

### IBridgeTheme

System theme (light/dark mode) detection.

```csharp
public interface IBridgeTheme
{
    ThemeMode Theme { get; }
    event EventHandler<ThemeMode>? ThemeChanged;
    Task InitializeAsync();
}
```

### IBridgeSafeArea

Safe area insets for notched/cutout devices.

```csharp
public interface IBridgeSafeArea
{
    SafeAreaInsets SafeArea { get; }
    event EventHandler<SafeAreaInsets>? SafeAreaChanged;
    Task InitializeAsync();
}
```

---

## Enums

### Host

```csharp
public enum Host
{
    Unknown,
    Maui,
    Blazor,
    Wpf,
    WinForms,
}
```

### PlatformIdentity

```csharp
public enum PlatformIdentity
{
    Unknown,
    Android,
    IOS,
    Windows,
    Mac,
    Linux,
    Web,
}
```

### FormFactor

```csharp
public enum FormFactor
{
    Unknown,
    Phone,
    Tablet,
    Desktop,
}
```

### ResizeMode

```csharp
public enum ResizeMode
{
    None,    // Components manage their own listeners
    Global,  // Single persistent shared listener
    Once,    // Read once, no ongoing listening
}
```

### ThemeMode

```csharp
public enum ThemeMode
{
    Unknown,
    Light,
    Dark,
}
```

---

## Records and Models

### FormFactorInfo

```csharp
public sealed record FormFactorInfo(FormFactor FormFactor, double Width, double Height)
{
    public static FormFactorInfo Unknown();
    public static FormFactorInfo Unknown(double width, double height);
}
```

### SafeAreaInsets

```csharp
public sealed record SafeAreaInsets(double Top, double Right, double Bottom, double Left)
{
    public static SafeAreaInsets Zero { get; }
    public bool HasInsets { get; }
}
```

### ConnectivityOptions

```csharp
public sealed class ConnectivityOptions
{
    public int IntervalInSeconds { get; set; } = 10;     // Polling interval (Blazor only)
    public string TestUrl { get; set; } = "/favicon.ico"; // URL to ping (Blazor only)
}
```

---

## Components

All components are in the `Circuids.Bridge` namespace.

| Component | RenderFragments | Context Type |
|-----------|----------------|--------------|
| `<BridgeHost>` | `Maui`, `Blazor`, `Wpf`, `WinForms`, `Default` | `Host` |
| `<BridgePlatform>` | `Android`, `IOS`, `Windows`, `Mac`, `Linux`, `Web`, `Default` | `PlatformIdentity` |
| `<BridgeFormFactor>` | `Phone`, `Tablet`, `Desktop`, `DesktopAndTablet`, `DesktopAndPhone`, `TabletAndPhone`, `Default` | `FormFactorInfo` |
| `<BridgeConnectivity>` | `Online`, `Offline` | `bool` |
| `<BridgeTheme>` | `Light`, `Dark`, `Default` | `ThemeMode` |
| `<BridgeSafeArea>` | -- | `SafeAreaInsets` |

---

## Providers

| Provider | Parameters | Description |
|----------|-----------|-------------|
| `<BridgeProvider>` | `FormFactorResizeMode`, `ConnectivityOptions` | Composite -- initializes all services. |
| `<BridgeFormFactorProvider>` | `Mode` (`ResizeMode`) | Initializes `IBridgeFormFactor`. |
| `<BridgeConnectivityProvider>` | `Options` (`ConnectivityOptions?`) | Initializes `IBridgeConnectivity`. |
| `<BridgeThemeProvider>` | -- | Initializes `IBridgeTheme`. |
| `<BridgeSafeAreaProvider>` | -- | Initializes `IBridgeSafeArea`. |

---

## Handlers

### BridgeHostHandler\<T>

```csharp
public abstract class BridgeHostHandler<T>(IBridge bridge)
{
    protected abstract T OnMaui();
    protected abstract T OnBlazor();
    protected virtual T OnWpf() => OnBlazor();
    protected virtual T OnWinForms() => OnBlazor();
    protected virtual T OnUnknown() => throw new BridgeException(...);
    public T Execute();
}
```

### BridgeHostHandler

```csharp
public abstract class BridgeHostHandler(IBridge bridge)
{
    protected abstract void OnMaui();
    protected abstract void OnBlazor();
    protected virtual void OnWpf() => OnBlazor();
    protected virtual void OnWinForms() => OnBlazor();
    protected virtual void OnUnknown() => throw new BridgeException(...);
    public void Execute();
}
```

---

## Exceptions

### BridgeException

```csharp
public sealed class BridgeException : Exception
{
    public BridgeException(string message);
    public BridgeException(string message, Exception innerException);
}
```

---

## Extension Methods

### Circuids.Bridge.Blazor

```csharp
public static IServiceCollection AddBridgeForBlazor(this IServiceCollection services);
```

### Circuids.Bridge.Maui

```csharp
public static IServiceCollection AddBridgeForMaui(this IServiceCollection services);
```

---

# License

**Circuids Bridge** is Licensed Under [MIT License](https://github.com/Circuids/Bridge/blob/main/LICENSE).

---

# Contributing

Contributions are welcome. If you wish to contribute to this project, please don't hesitate to create an issue or submit a pull request. Your input and feedback are highly appreciated.

Before submitting a PR:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request with a clear description

---

# Sponsoring

If you find this project useful and would like to support its continued development, consider [becoming a sponsor](https://github.com/sponsors/AathifMahir). Your contributions are instrumental in keeping this project maintained and growing. Thank you for your support.