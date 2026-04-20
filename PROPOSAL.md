# Circuids Bridge — Rebranding, Architecture & Expansion Proposal

> **Author:** Aathif Mahir / Circuids  
> **Date:** April 2026  
> **Status:** Draft v2  
> **Source:** MauiBlazorBridge v1.0.0-preview15 → Circuids.Bridge v1.0.0  
> **Target Framework:** .NET 10  
> **Namespace Convention:** [Unified Namespace Convention for Circuids Libraries](https://copilot.microsoft.com/shares/pages/VBCdTQ6DbKCywEAnuLDWF)

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Bug Report — Existing MauiBlazorBridge Issues](#2-bug-report--existing-mauiblazorbridge-issues)
3. [Architecture Critique — Current Design](#3-architecture-critique--current-design)
4. [Namespace Convention — Applied](#4-namespace-convention--applied)
5. [Package Decoupling Strategy](#5-package-decoupling-strategy)
6. [Provider Design — Single vs Modular](#6-provider-design--single-vs-modular)
7. [Naming Improvements](#7-naming-improvements)
8. [Proposed Architecture — Circuids Bridge](#8-proposed-architecture--circuids-bridge)
9. [Feature Streamlining & Expansion](#9-feature-streamlining--expansion)
10. [Migration Plan](#10-migration-plan)
11. [Breaking Changes Summary](#11-breaking-changes-summary)

---

## 1. Executive Summary

MauiBlazorBridge is being rebranded as **Bridge** under the **Circuids** organization. This transition is an opportunity to:

- Fix 9 identified bugs including critical MAUI initialization failures
- Decouple the library into modular packages following the Unified Namespace Convention
- Move to .NET 10
- Streamline existing capabilities and expand with Theme detection and Safe Area support
- Design a provider model that balances convenience with resource efficiency

---

## 2. Bug Report — Existing MauiBlazorBridge Issues

### 2.1 Critical — MAUI Services Throw at Construction Time

**Files:** `Services/Bridge.cs`, `Services/BridgeFormFactor.cs`, `Services/BridgeConnectivity.cs`

All three MAUI service implementations access MAUI platform APIs **during construction** (i.e., when DI resolves the service), not during `InitializeAsync()`. This causes exceptions on certain platforms and timing scenarios.

| Service | Offending Code | Risk |
|---------|---------------|------|
| `Bridge` | `Platform = GetPlatform()` in property initializer — calls `DeviceInfo.Platform` | `DeviceInfo` may not be ready during DI resolution |
| `Bridge` | `PlatformVersion = DeviceInfo.Version.ToString()` in property initializer | Same — `DeviceInfo.Version` accessed at construction |
| `BridgeFormFactor` | `DeviceFormFactor = GetFormFactor()` in constructor — calls `Application.Current.Windows[0]` | `Application.Current` is `null` or has no windows during early DI resolution |
| `BridgeConnectivity` | `IsInternetConnected = Connectivity.NetworkAccess == NetworkAccess.Internet` in constructor | `Connectivity` may not be initialized on all platforms during DI |

**Root Cause:** The DI container constructs these as scoped services. On MAUI, scoped services can be resolved before the platform is fully initialized (before `Application.Current` has windows, before `DeviceInfo` is ready). These property initializers and constructor calls run immediately at construction time, not when `InitializeAsync()` is called.

**Fix:** Defer ALL platform API access to `InitializeAsync()`. Use safe defaults (`Unknown`, `false`, etc.) in constructors.

---

### 2.2 Critical — `BridgePlatform.razor` Mac Condition Bug

**File:** `Components/BridgePlatform.razor`

```csharp
// BUG: Uses "Windows is not null" instead of "Mac is not null"
else if (Bridge.Platform is PlatformIdentity.Mac && Windows is not null)
{
    @Mac
}
```

The Mac branch checks `Windows is not null` instead of `Mac is not null`, meaning if the `Mac` RenderFragment is provided but `Windows` is not, macOS content will never render.

---

### 2.3 Critical — `BridgePlatform.razor` Missing Default Fragment

Unlike `BridgeFormFactor` and `BridgeFramework`, `BridgePlatform` has no `Default` render fragment. When the platform is `Unknown` or doesn't match any of the four provided fragments, **nothing renders** — silent failure with no fallback.

---

### 2.4 High — `BridgeFormFactor.js` Resize Listener Variable Mismatch

**File:** `wwwroot/BridgeFormFactor.js`

```javascript
// BUG: Uses "window.currentIdiom" but sets "window.currentFormFactor" at initialization
window.currentFormFactor = getFormFactor();  // ← sets this

window.resizeListener = async () => {
    const formFactor = getFormFactor();
    if (window.currentIdiom !== formFactor) {  // ← checks this (different variable!)
        window.currentIdiom = formFactor;       // ← sets this (different variable!)
        await dotnetObject.invokeMethodAsync("NotifyFormFactorChanged", formFactor);
    }
};
```

`window.currentFormFactor` is set during initialization but `window.currentIdiom` is used in the resize listener comparison. This means the comparison against the initial value always fails (comparing against `undefined`), causing **every resize event** to fire `NotifyFormFactorChanged` even when the form factor hasn't changed.

---

### 2.5 High — Initialization Race Condition

**Provider Pattern:** Providers use `OnAfterRenderAsync(firstRender)` to call `InitializeAsync()`. But other components can inject `IBridge`, `IBridgeFormFactor`, etc., and access their properties before the provider has rendered.

**Example flow:**
1. Component A injects `IBridgeFormFactor` and reads `DeviceFormFactor` in `OnInitialized`
2. Component A renders before `BridgeFormFactorProvider` renders
3. `InitializeAsync()` hasn't been called yet → Component A gets stale/default values or exception

This is especially problematic with Blazor Server pre-rendering where component render order is non-deterministic.

---

### 2.6 Medium — MAUI `BridgeConnectivity` Never Unsubscribes

**File:** `Services/BridgeConnectivity.cs`

The service subscribes to `Connectivity.ConnectivityChanged` in `InitializeAsync()` but never unsubscribes. Since this is a scoped service, on navigation or circuit disposal, the event handler is leaked. The MAUI service doesn't implement `IDisposable` or `IAsyncDisposable`.

---

### 2.7 Medium — BridgeConnectivity.js Hardcoded Google URL

**File:** `wwwroot/BridgeConnectivity.js`

```javascript
const testUrl = 'https://www.google.com/generate_204';
```

This fails in regions where Google is blocked (China, corporate firewalls, etc.). The URL should be configurable.

---

### 2.8 Low — `BridgeFormFactor` MAUI Global Mode Missing `await`

**File:** `Services/BridgeFormFactor.cs`

```csharp
if (listenerType is ChangeListeningMode.Global)
    CreateAsync();  // ← fire-and-forget, no await
```

The `CreateAsync()` call in Global mode is not awaited. While it's currently synchronous in MAUI, this is a correctness issue and breaks the async contract.

---

### 2.9 Low — Thread Safety Concerns

Multiple MAUI service properties are read/written from UI thread and Blazor thread without synchronization. `DeviceFormFactor`, `IsInternetConnected`, `Platform`, etc., are all set on one thread and potentially read on another.

---

## 3. Architecture Critique — Current Design

### 3.1 Preprocessor Directive Coupling

The entire architecture relies on `#if ANDROID || IOS || WINDOWS || MACCATALYST` to switch between MAUI and Web implementations. This approach has fundamental limitations:

- **Cannot support WPF BlazorWebView** — WPF targets `net10.0-windows` but isn't MAUI, so neither the MAUI code (`#if WINDOWS` refers to MAUI WinUI, not WPF) nor the Web code is correct
- **Cannot support WinForms BlazorWebView** — Same issue
- **Multi-targeting complexity** — The `.csproj` must enumerate every platform TFM
- **Testing difficulty** — Cannot mock or swap implementations without compilation flags
- **Single assembly** — All platform code ships in one package even if unused

### 3.2 Provider Pattern Fragility

The `BridgeProvider`, `BridgeFormFactorProvider`, and `BridgeConnectivityProvider` are separate components that must be placed in the render tree. This creates:

- **Three separate initialization points** with no coordination
- **Render-order dependency** — providers must render before consumers
- **Easy to forget** — developers must remember to add 3 provider components
- **Pre-rendering incompatibility** — providers don't fire during SSR, leading to inconsistent states

### 3.3 Missing Abstraction Layer

There is no host abstraction. The library directly talks to either MAUI APIs or JavaScript. To support WPF/WinForms, there needs to be an abstraction layer that each host implements.

---

## 4. Namespace Convention — Applied

Following the [Unified Namespace Convention for Circuids Libraries](https://copilot.microsoft.com/shares/pages/VBCdTQ6DbKCywEAnuLDWF):

### 4.1 Bridge Falls Under Two Categories

| Category | Application |
|----------|-------------|
| **1. Host-Agnostic Core** | `Circuids.Bridge` — Core interfaces, enums, Razor components, handler abstractions |
| **2. Host-Specific Implementations** | `Circuids.Bridge.Maui`, `Circuids.Bridge.Blazor`, `Circuids.Bridge.Wpf`, `Circuids.Bridge.WinForms` |

### 4.2 Package → Namespace Mapping

| NuGet Package ID | Root Namespace | Description |
|-----------------|---------------|-------------|
| `Circuids.Bridge` | `Circuids.Bridge` | Core abstractions, components, enums |
| `Circuids.Bridge.Blazor` | `Circuids.Bridge.Blazor` | Blazor WASM/Server JS interop implementations |
| `Circuids.Bridge.Maui` | `Circuids.Bridge.Maui` | MAUI platform implementations |
| `Circuids.Bridge.Wpf` | `Circuids.Bridge.Wpf` | WPF implementations (future) |
| `Circuids.Bridge.WinForms` | `Circuids.Bridge.WinForms` | WinForms implementations (future) |

### 4.3 Why Blazor Is a Separate Host Package

In the previous proposal, Blazor Web implementations lived inside the core `Circuids.Bridge` package. The updated convention treats Blazor as a host-specific implementation, which is the correct design:

- **Core should be pure abstractions** — no JS interop, no `IJSRuntime`, no web-specific logic
- **Blazor WASM and Blazor Server are hosts** just like MAUI or WPF
- **Shared RCLs** (Razor Class Libraries shared between hosts) reference `Circuids.Bridge` only — they get interfaces and components without JS or MAUI dependencies
- **Consistency** — every host is treated equally, no host gets special "bundled-in-core" treatment

### 4.4 Developer Experience by Scenario

```csharp
// Blazor WASM/Server → install Circuids.Bridge.Blazor (pulls core transitively)
builder.Services.AddBridgeForBlazor();

// MAUI Blazor Hybrid → install Circuids.Bridge.Maui (pulls core transitively)
builder.Services.AddBridgeForMaui();

// Shared RCL → install Circuids.Bridge only (interfaces + components)
@using Circuids.Bridge
@inject IBridge Bridge

// WPF Blazor Hybrid (future) → install Circuids.Bridge.Wpf
services.AddBridgeForWpf();
```

---

## 5. Package Decoupling Strategy

### 5.1 Package Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Circuids.Bridge                            │
│                (Host-Agnostic Core — net10.0)                   │
│                                                                 │
│  Abstractions: IBridge, IBridgeFormFactor, IBridgeConnectivity, │
│                IBridgeTheme, IBridgeSafeArea                    │
│  Enums: Host, FormFactor, PlatformIdentity, ThemeMode, etc.    │
│  Records: DeviceFormFactor, SafeAreaInsets                      │
│  Components: BridgeFormFactor, BridgePlatform, BridgeHost,     │
│              BridgeConnectivity, BridgeTheme                    │
│  Providers: BridgeProvider, BridgeFormFactorProvider,           │
│             BridgeConnectivityProvider, BridgeThemeProvider,    │
│             BridgeSafeAreaProvider                              │
│  Handlers: BridgeHostHandler, BridgeHostHandler<T>             │
│  Exception: BridgeException                                    │
│                                                                 │
│  NO implementations — pure abstractions + Razor components     │
└─────────────┬────────────────────────┬──────────────────────────┘
              │                        │
     ┌────────┴────────┐      ┌────────┴────────┐
     │                 │      │                 │
     ▼                 ▼      ▼                 ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│Circuids.     │ │Circuids.     │ │Circuids.     │ │Circuids.     │
│Bridge.Blazor │ │Bridge.Maui   │ │Bridge.Wpf    │ │Bridge.       │
│              │ │              │ │              │ │WinForms      │
│net10.0       │ │net10.0-and.. │ │net10.0-win.. │ │net10.0-win.. │
│              │ │net10.0-ios   │ │              │ │              │
│JS interop    │ │net10.0-mac.. │ │WPF APIs      │ │WinForms APIs │
│implementations│ │net10.0-win.. │ │              │ │              │
│              │ │              │ │AddBridgeFor  │ │AddBridgeFor  │
│AddBridgeFor  │ │MAUI APIs     │ │  Wpf()       │ │  WinForms()  │
│  Blazor()    │ │              │ │              │ │              │
│              │ │AddBridgeFor  │ │   (future)   │ │   (future)   │
│              │ │  Maui()      │ │              │ │              │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
```

### 5.2 Package Responsibilities

| Package | TFM | Contents |
|---------|-----|----------|
| `Circuids.Bridge` | `net10.0` | Pure abstractions (interfaces, enums, records), Razor components, provider components, handler base classes, exception. Zero platform dependencies — only `Microsoft.AspNetCore.Components.Web`. |
| `Circuids.Bridge.Blazor` | `net10.0` | Blazor WASM/Server implementations using JS interop. All `.js` files live here. Registers web-based `IBridge`, `IBridgeFormFactor`, `IBridgeConnectivity`, `IBridgeTheme`, `IBridgeSafeArea`. |
| `Circuids.Bridge.Maui` | `net10.0-android/ios/maccatalyst/windows` | MAUI implementations using `DeviceInfo`, `Connectivity`, `Application.Current`, etc. References `Microsoft.Maui.Controls`. |
| `Circuids.Bridge.Wpf` *(v2)* | `net10.0-windows` | WPF BlazorWebView implementations. |
| `Circuids.Bridge.WinForms` *(v2)* | `net10.0-windows` | WinForms BlazorWebView implementations. |

### 5.3 Why This Split Works

1. **Shared RCLs** reference `Circuids.Bridge` only — get interfaces + components, no platform baggage
2. **Each host is equal** — Blazor, MAUI, WPF all follow the same pattern
3. **No `#if` preprocessor directives** — each package compiles for its target only
4. **Testing** — mock interfaces in `Circuids.Bridge`, no need for platform-specific test projects
5. **Tree-shaking** — only ship the host you actually use

---

## 6. Provider Design — Single vs Modular

### 6.1 The Trade-off

Your original design moved from a single provider to three separate providers (`BridgeProvider`, `BridgeFormFactorProvider`, `BridgeConnectivityProvider`) so developers could initialize only what they need. This is a valid resource-efficiency concern. Let's evaluate both:

| Aspect | Single Provider | Modular Providers |
|--------|----------------|-------------------|
| **Simplicity** | One component, one line in layout | Must remember multiple providers |
| **Resource efficiency** | Initializes everything including unused features | Only initializes what's needed |
| **Initialization order** | Guaranteed — sequential in one place | Developer must ensure correct render order |
| **Configuration** | One place for all settings | Distributed configuration |
| **Discoverability** | Easy — one provider to learn | Must discover each provider separately |
| **Pre-rendering** | One initialization point to guard | Multiple points to handle |
| **Feature growth** | Every new feature adds a parameter | Every new feature adds a provider |

### 6.2 Recommendation: Hybrid Approach — Modular Providers + Convenience Composite

Keep individual providers for fine-grained control **and** offer a composite `BridgeProvider` that wraps them all for convenience. This gives both power users and simple-use-case developers what they want.

```
BridgeProvider (composite — convenience)
├── wraps BridgeCoreProvider        (always initializes — platform, host detection)
├── wraps BridgeFormFactorProvider  (opt-in via parameter)
├── wraps BridgeConnectivityProvider (opt-in via parameter)
├── wraps BridgeThemeProvider        (opt-in via parameter)
└── wraps BridgeSafeAreaProvider     (opt-in via parameter)
```

#### Usage — Simple (composite provider, everything enabled by default):

```razor
<BridgeProvider>
    <Router AppAssembly="typeof(App).Assembly">
        ...
    </Router>
</BridgeProvider>
```

#### Usage — Selective (composite, disable what you don't need):

```razor
<BridgeProvider EnableConnectivity="false" EnableSafeArea="false">
    ...
</BridgeProvider>
```

#### Usage — Granular (individual providers for maximum control):

```razor
<BridgeCoreProvider>
    <BridgeFormFactorProvider Mode="ResizeMode.Global">
        <BridgeThemeProvider>
            <Router AppAssembly="typeof(App).Assembly">
                ...
            </Router>
        </BridgeThemeProvider>
    </BridgeFormFactorProvider>
</BridgeCoreProvider>
```

#### Implementation — Composite `BridgeProvider`:

```razor
@namespace Circuids.Bridge

@if (_isInitialized)
{
    @ChildContent
}

@code {
    [Inject] private IBridge Bridge { get; set; } = default!;
    [Inject] private IBridgeFormFactor FormFactor { get; set; } = default!;
    [Inject] private IBridgeConnectivity Connectivity { get; set; } = default!;
    [Inject] private IBridgeTheme Theme { get; set; } = default!;
    [Inject] private IBridgeSafeArea SafeArea { get; set; } = default!;

    [Parameter] public RenderFragment ChildContent { get; set; } = default!;

    // Feature toggles — all enabled by default
    [Parameter] public bool EnableFormFactor { get; set; } = true;
    [Parameter] public bool EnableConnectivity { get; set; } = true;
    [Parameter] public bool EnableTheme { get; set; } = true;
    [Parameter] public bool EnableSafeArea { get; set; } = true;

    // Configuration
    [Parameter] public ResizeMode FormFactorResizeMode { get; set; } = ResizeMode.None;
    [Parameter] public ConnectivityOptions? ConnectivityOptions { get; set; }

    private bool _isInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Core always initializes (platform + host detection)
            await Bridge.InitializeAsync();

            if (EnableFormFactor)
                await FormFactor.InitializeAsync(FormFactorResizeMode);

            if (EnableConnectivity)
                await Connectivity.InitializeAsync(ConnectivityOptions);

            if (EnableTheme)
                await Theme.InitializeAsync();

            if (EnableSafeArea)
                await SafeArea.InitializeAsync();

            _isInitialized = true;
            StateHasChanged();
        }
    }
}
```

### 6.3 Why This Design

- **Simple case is simple:** `<BridgeProvider>` with no parameters initializes everything — one line
- **Opt-out is easy:** `EnableConnectivity="false"` skips connectivity without touching other features
- **Power users get control:** Individual providers for complex layouts, pre-rendering scenarios, or resource-sensitive apps
- **No resource waste:** Disabled features don't initialize (no JS modules loaded, no event listeners created, no polling)
- **Future-proof:** New features (SafeArea, Theme) get an `Enable*` parameter on the composite and their own provider — existing code doesn't break

---

## 7. Naming Improvements

### 7.1 Construct Renames

| Old Name | New Name | Rationale |
|----------|----------|-----------|
| `ChangeListeningMode` | `ResizeMode` | The enum only applies to form factor resize listening. "ChangeListeningMode" is generic and unclear about *what* changes are being listened to. `ResizeMode` is specific and self-documenting. |
| `ChangeListeningMode.None` | `ResizeMode.None` | No listener attached at init; components create/dispose their own |
| `ChangeListeningMode.Global` | `ResizeMode.Global` | Single persistent listener, shared across all components |
| `ChangeListeningMode.Suppressed` | `ResizeMode.Once` | "Suppressed" sounds like an error state or something disabled. `Once` clearly communicates: read the value once at init, no ongoing listening. |
| `Framework` enum | `Host` enum | "Framework" is ambiguous (.NET itself is a framework). "Host" precisely describes what's being detected — the hosting environment. |
| `Framework.Maui` / `.Blazor` | `Host.Maui` / `.Blazor` / `.Wpf` / `.WinForms` | Extended with new host types |
| `BridgeFramework` component | `BridgeHost` | Follows the enum rename |
| `BridgeFrameworkHandler<T>` | `BridgeHostHandler<T>` | Follows the enum rename |
| `MauiBlazorBridgeException` | `BridgeException` | Clean, no legacy baggage |
| `BridgeFormFactorContext` | *Removed* | Over-engineered wrapper; components subscribe to events directly |
| `IBridge.Framework` | `IBridge.Host` | Property follows enum rename |
| `IBridgeFormFactor.DisposeFormFactor()` | `IBridgeFormFactor.DisposeListenerAsync()` | Clearer — it disposes the *listener*, not the form factor |
| `IBridgeFormFactor.CreateAsync()` | `IBridgeFormFactor.CreateListenerAsync()` | Clearer — creates a resize *listener* |
| `IBridgeConnectivity.InternetConnectionChanged` | `IBridgeConnectivity.ConnectionChanged` | Shorter, less redundant |
| `IBridgeConnectivity.IsInternetConnected` | `IBridgeConnectivity.IsConnected` | Shorter |
| `DeviceFormFactor` record | `FormFactorInfo` | "DeviceFormFactor" is redundant (form factor already implies device). `FormFactorInfo` is clean and matches .NET convention (`DeviceInfo`, `DisplayInfo`). |
| `ConnectivityIntervalInSeconds` parameter | Part of `ConnectivityOptions` | Moved into options object for cleaner API |

### 7.2 Enum Value Additions

```csharp
public enum Host
{
    Unknown,
    Maui,
    Blazor,
    Wpf,
    WinForms,
}

public enum PlatformIdentity
{
    Unknown,
    Android,
    IOS,
    Windows,
    Mac,
    Linux,      // Blazor on Linux browsers
    Web,        // Generic web platform (when specific OS can't be determined)
}

public enum ThemeMode
{
    Unknown,
    Light,
    Dark,
}

public enum ResizeMode      // was ChangeListeningMode
{
    None,       // No listener; components manage their own
    Global,     // Persistent shared listener
    Once,       // Read once at init, no ongoing listening (was "Suppressed")
}
```

---

## 8. Proposed Architecture — Circuids Bridge

### 8.1 Core Package: `Circuids.Bridge`

```
Circuids.Bridge/
├── Circuids.Bridge.csproj                  (net10.0, Razor SDK)
├── _Imports.razor
├── Abstractions/
│   ├── IBridge.cs
│   ├── IBridgeFormFactor.cs
│   ├── IBridgeConnectivity.cs
│   ├── IBridgeTheme.cs
│   └── IBridgeSafeArea.cs
├── Common/
│   ├── Host.cs                              (enum — was Framework)
│   ├── FormFactor.cs                        (enum)
│   ├── FormFactorInfo.cs                    (record — was DeviceFormFactor)
│   ├── PlatformIdentity.cs                 (enum — expanded)
│   ├── ResizeMode.cs                        (enum — was ChangeListeningMode)
│   ├── ThemeMode.cs                         (enum)
│   ├── SafeAreaInsets.cs                    (record)
│   └── ConnectivityOptions.cs               (class)
├── Components/
│   ├── BridgeFormFactor.razor
│   ├── BridgePlatform.razor                 (bug-fixed)
│   ├── BridgeHost.razor                     (was BridgeFramework)
│   ├── BridgeConnectivity.razor
│   ├── BridgeTheme.razor
│   └── BridgeSafeArea.razor
├── Providers/
│   ├── BridgeProvider.razor                 (composite — convenience)
│   ├── BridgeCoreProvider.razor             (platform + host only)
│   ├── BridgeFormFactorProvider.razor
│   ├── BridgeConnectivityProvider.razor
│   ├── BridgeThemeProvider.razor
│   └── BridgeSafeAreaProvider.razor
├── Handlers/
│   ├── BridgeHostHandler.cs                 (was BridgeFrameworkHandler — void)
│   └── BridgeHostHandler{T}.cs              (generic return)
├── Exceptions/
│   └── BridgeException.cs
└── readme.md
```

### 8.2 Blazor Host Package: `Circuids.Bridge.Blazor`

```
Circuids.Bridge.Blazor/
├── Circuids.Bridge.Blazor.csproj           (net10.0, Razor SDK)
├── Internal/
│   ├── BridgeBlazor.cs                      (IBridge via JS)
│   ├── BridgeFormFactorBlazor.cs            (IBridgeFormFactor via JS)
│   ├── BridgeConnectivityBlazor.cs          (IBridgeConnectivity via JS)
│   ├── BridgeThemeBlazor.cs                 (IBridgeTheme via JS)
│   └── BridgeSafeAreaBlazor.cs              (IBridgeSafeArea via JS)
├── Extensions/
│   └── BridgeBlazorServiceExtensions.cs     (AddBridgeForBlazor())
└── wwwroot/
    ├── Bridge.js
    ├── BridgeFormFactor.js                  (bug-fixed)
    ├── BridgeConnectivity.js                (configurable URL)
    ├── BridgeTheme.js
    └── BridgeSafeArea.js
```

### 8.3 MAUI Host Package: `Circuids.Bridge.Maui`

```
Circuids.Bridge.Maui/
├── Circuids.Bridge.Maui.csproj             (net10.0-android/ios/maccatalyst/windows)
├── Internal/
│   ├── BridgeMaui.cs                        (IBridge — deferred init, fixed)
│   ├── BridgeFormFactorMaui.cs              (IBridgeFormFactor — deferred init, fixed)
│   ├── BridgeConnectivityMaui.cs            (IBridgeConnectivity — IDisposable, fixed)
│   ├── BridgeThemeMaui.cs                   (IBridgeTheme — AppTheme API)
│   └── BridgeSafeAreaMaui.cs                (IBridgeSafeArea — platform-specific)
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
└── Extensions/
    └── BridgeMauiServiceExtensions.cs       (AddBridgeForMaui())
```

### 8.4 Revised Interfaces

```csharp
namespace Circuids.Bridge;

public interface IBridge
{
    Host Host { get; }
    PlatformIdentity Platform { get; }
    string PlatformVersion { get; }
    bool IsInitialized { get; }
    event EventHandler<PlatformIdentity>? PlatformChanged;
    Task InitializeAsync();
}

public interface IBridgeFormFactor
{
    FormFactorInfo FormFactor { get; }
    event EventHandler<FormFactorInfo>? FormFactorChanged;
    Task InitializeAsync(ResizeMode resizeMode = ResizeMode.None);
    Task CreateListenerAsync();
    ValueTask DisposeListenerAsync();
}

public interface IBridgeConnectivity
{
    bool IsConnected { get; }
    event EventHandler<bool>? ConnectionChanged;
    Task InitializeAsync(ConnectivityOptions? options = null);
}

public interface IBridgeTheme
{
    ThemeMode Theme { get; }
    event EventHandler<ThemeMode>? ThemeChanged;
    Task InitializeAsync();
}

public interface IBridgeSafeArea
{
    SafeAreaInsets SafeArea { get; }
    event EventHandler<SafeAreaInsets>? SafeAreaChanged;
    Task InitializeAsync();
}
```

### 8.5 Revised Records and Models

```csharp
namespace Circuids.Bridge;

/// <summary>
/// Describes the current form factor and viewport dimensions.
/// </summary>
public sealed record FormFactorInfo(FormFactor FormFactor, double Width, double Height)
{
    public static FormFactorInfo Unknown() => new(FormFactor.Unknown, 0, 0);
    public static FormFactorInfo Unknown(double width, double height) => new(FormFactor.Unknown, width, height);
}

/// <summary>
/// Safe area insets for notched/cutout devices.
/// All values in CSS pixels (device-independent).
/// </summary>
public sealed record SafeAreaInsets(double Top, double Right, double Bottom, double Left)
{
    public static SafeAreaInsets Zero => new(0, 0, 0, 0);
}

/// <summary>
/// Configuration for connectivity monitoring.
/// </summary>
public sealed class ConnectivityOptions
{
    /// <summary>
    /// Polling interval for web-based connectivity checks.
    /// Ignored on MAUI (uses native ConnectivityChanged event).
    /// Default: 10 seconds.
    /// </summary>
    public int IntervalInSeconds { get; set; } = 10;

    /// <summary>
    /// URL to ping for connectivity verification on web.
    /// Default: "/favicon.ico" (self-hosted, avoids external dependencies).
    /// </summary>
    public string TestUrl { get; set; } = "/favicon.ico";
}
```

### 8.6 Handler Design (Streamlined)

Reduced from 4 variants to 2. Async use cases return `Task<T>` or `Task` from the sync handler methods.

```csharp
namespace Circuids.Bridge;

/// <summary>
/// Executes different logic depending on the detected host environment.
/// </summary>
public abstract class BridgeHostHandler<T>(IBridge bridge)
{
    protected abstract T OnMaui();
    protected abstract T OnBlazor();
    protected virtual T OnWpf() => OnBlazor();         // default fallback
    protected virtual T OnWinForms() => OnBlazor();     // default fallback
    protected virtual T OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public T Execute() => bridge.Host switch
    {
        Host.Maui => OnMaui(),
        Host.Blazor => OnBlazor(),
        Host.Wpf => OnWpf(),
        Host.WinForms => OnWinForms(),
        _ => OnUnknown(),
    };
}

/// <summary>
/// Void variant — executes host-specific side effects.
/// </summary>
public abstract class BridgeHostHandler(IBridge bridge)
{
    protected abstract void OnMaui();
    protected abstract void OnBlazor();
    protected virtual void OnWpf() => OnBlazor();
    protected virtual void OnWinForms() => OnBlazor();
    protected virtual void OnUnknown() =>
        throw new BridgeException("Host is Unknown. Ensure Bridge is initialized via BridgeProvider.");

    public void Execute()
    {
        switch (bridge.Host)
        {
            case Host.Maui: OnMaui(); break;
            case Host.Blazor: OnBlazor(); break;
            case Host.Wpf: OnWpf(); break;
            case Host.WinForms: OnWinForms(); break;
            default: OnUnknown(); break;
        }
    }
}
```

**Key design improvements:**
- `OnWpf()` and `OnWinForms()` default to `OnBlazor()` — new hosts work without overriding every method, since WPF/WinForms BlazorWebView behavior is often similar to Blazor
- `OnUnknown()` is virtual — can be overridden for custom fallback instead of always throwing
- Primary constructors used (C# 12+)

---

## 9. Feature Streamlining & Expansion

### 9.1 Streamlining Summary

| Current | New | Rationale |
|---------|-----|-----------|
| `BridgeFrameworkHandler<T>` × 4 variants | `BridgeHostHandler<T>` + `BridgeHostHandler` (2) | Async handled via `Task<T>` return. `OnWpf()`/`OnWinForms()` default to `OnBlazor()`. |
| `BridgeFormFactorContext` class | Removed | Over-engineered. Components subscribe to `IBridgeFormFactor.FormFactorChanged` directly. |
| Three separate providers | Modular providers + composite `BridgeProvider` | Best of both worlds (see Section 6). |
| Hardcoded 10-second dispose delay | Configurable debounce (default 5s) | Via parameter on `IBridgeFormFactor` |
| Hardcoded Google URL for connectivity | Self-hosted default (`/favicon.ico`) + configurable | Via `ConnectivityOptions.TestUrl` |
| `PlatformVersion` = "Unknown" on web | Parse `navigator.userAgent` | Extract OS version from user agent string |

### 9.2 New Feature: Theme/Appearance Detection

**Interface:** `IBridgeTheme`

| Host | Implementation |
|------|---------------|
| **Blazor** | JS interop: `window.matchMedia('(prefers-color-scheme: dark)')` with `change` listener |
| **MAUI** | `Application.Current.RequestedTheme` + `Application.Current.RequestedThemeChanged` |
| **WPF** | Registry `HKCU\...\Themes\Personalize\AppsUseLightTheme` + `SystemEvents.UserPreferenceChanged` |

**Component:**

```razor
<BridgeTheme>
    <Light>
        <p>Light mode active</p>
    </Light>
    <Dark>
        <p>Dark mode active</p>
    </Dark>
    <Default>
        <p>Theme unknown</p>
    </Default>
</BridgeTheme>

<!-- Or with ChildContent for custom logic -->
<BridgeTheme>
    @context  <!-- ThemeMode enum value -->
</BridgeTheme>
```

### 9.3 New Feature: Safe Area Insets

**Interface:** `IBridgeSafeArea`

| Host | Implementation |
|------|---------------|
| **Blazor** | JS interop: read CSS `env(safe-area-inset-top)`, `env(safe-area-inset-right)`, etc. Listen for `orientationchange` and `resize` to update. |
| **MAUI (iOS)** | `UIApplication.SharedApplication.KeyWindow.SafeAreaInsets` via platform-specific code |
| **MAUI (Android)** | `WindowInsets` API via `ViewCompat.GetRootWindowInsets()` |
| **MAUI (Windows/Mac)** | Return `SafeAreaInsets.Zero` (no notches) |

**Record:**

```csharp
public sealed record SafeAreaInsets(double Top, double Right, double Bottom, double Left)
{
    public static SafeAreaInsets Zero => new(0, 0, 0, 0);
    
    /// <summary>
    /// Whether any inset is non-zero (i.e., device has notch/cutout/nav bar).
    /// </summary>
    public bool HasInsets => Top > 0 || Right > 0 || Bottom > 0 || Left > 0;
}
```

**Component:**

```razor
<BridgeSafeArea>
    <div style="padding-top: @(context.Top)px; padding-bottom: @(context.Bottom)px;">
        @ChildContent
    </div>
</BridgeSafeArea>
```

**JavaScript (Blazor host):**

```javascript
export function getSafeAreaInsets() {
    const style = getComputedStyle(document.documentElement);
    return JSON.stringify({
        Top: parseFloat(style.getPropertyValue('env(safe-area-inset-top)')) || 0,
        Right: parseFloat(style.getPropertyValue('env(safe-area-inset-right)')) || 0,
        Bottom: parseFloat(style.getPropertyValue('env(safe-area-inset-bottom)')) || 0,
        Left: parseFloat(style.getPropertyValue('env(safe-area-inset-left)')) || 0,
    });
}
```

> **Note:** `env(safe-area-inset-*)` requires `<meta name="viewport" content="viewport-fit=cover">` in the HTML. The documentation should call this out.

### 9.4 Expanded `BridgeHost` Component

```razor
<BridgeHost>
    <Maui>Running in MAUI</Maui>
    <Blazor>Running in Blazor</Blazor>
    <Wpf>Running in WPF</Wpf>
    <WinForms>Running in WinForms</WinForms>
    <Default>Unknown host</Default>
</BridgeHost>
```

### 9.5 Fixed `BridgePlatform` Component

```razor
<BridgePlatform>
    <Android>Android</Android>
    <IOS>iOS</IOS>
    <Windows>Windows</Windows>
    <Mac>macOS</Mac>
    <Linux>Linux</Linux>
    <Web>Web</Web>
    <Default>Unknown platform</Default>
</BridgePlatform>
```

Bug fixes:
- Mac branch now correctly checks `Mac is not null` (not `Windows is not null`)
- `Default` fragment added
- `Linux` and `Web` fragments added

---

## 10. Migration Plan

### Phase 1: Core Package (`Circuids.Bridge`)
- [ ] Set up project structure (already scaffolded)
- [ ] Define all interfaces: `IBridge`, `IBridgeFormFactor`, `IBridgeConnectivity`, `IBridgeTheme`, `IBridgeSafeArea`
- [ ] Define all enums: `Host`, `FormFactor`, `PlatformIdentity`, `ResizeMode`, `ThemeMode`
- [ ] Define records: `FormFactorInfo`, `SafeAreaInsets`, `ConnectivityOptions`
- [ ] Implement `BridgeException`
- [ ] Port all Razor components with bug fixes
- [ ] Implement both provider patterns (composite + individual)
- [ ] Implement `BridgeHostHandler<T>` and `BridgeHostHandler`

### Phase 2: Blazor Host Package (`Circuids.Bridge.Blazor`)
- [ ] Create project, reference `Circuids.Bridge`
- [ ] Port Blazor/JS implementations with all bug fixes
- [ ] Fix `BridgeFormFactor.js` variable mismatch (`currentIdiom` → `currentFormFactor`)
- [ ] Replace hardcoded Google URL with configurable `ConnectivityOptions.TestUrl`
- [ ] Implement `BridgeThemeBlazor` + `BridgeTheme.js`
- [ ] Implement `BridgeSafeAreaBlazor` + `BridgeSafeArea.js`
- [ ] Implement `AddBridgeForBlazor()` extension

### Phase 3: MAUI Host Package (`Circuids.Bridge.Maui`)
- [ ] Set up project (already scaffolded with platform folders)
- [ ] Port MAUI implementations with **all constructor bugs fixed** (deferred init)
- [ ] Add `IDisposable` to MAUI connectivity (unsubscribe from `Connectivity.ConnectivityChanged`)
- [ ] Fix missing `await` in Global mode
- [ ] Implement `BridgeThemeMaui` using `Application.RequestedTheme`
- [ ] Implement `BridgeSafeAreaMaui` using platform-specific APIs
- [ ] Implement `AddBridgeForMaui()` extension

### Phase 4: Samples & Documentation
- [ ] Blazor WASM sample (references `Circuids.Bridge.Blazor`)
- [ ] Blazor Server sample (references `Circuids.Bridge.Blazor`)
- [ ] MAUI Blazor Hybrid sample (references `Circuids.Bridge.Maui`)
- [ ] Shared RCL sample (references `Circuids.Bridge` only)
- [ ] Migration guide from `MauiBlazorBridge`

### Phase 5: Future *(post-v1)*
- [ ] `Circuids.Bridge.Wpf`
- [ ] `Circuids.Bridge.WinForms`

---

## 11. Breaking Changes Summary

| Area | Old (`MauiBlazorBridge`) | New (`Circuids.Bridge`) |
|------|------------------------|------------------------|
| Root Namespace | `MauiBlazorBridge` | `Circuids.Bridge` |
| NuGet Package | `AathifMahir.MauiBlazor.MauiBlazorBridge` | `Circuids.Bridge` + `Circuids.Bridge.Blazor` or `Circuids.Bridge.Maui` |
| DI Registration | `AddMauiBlazorBridge()` | `AddBridgeForBlazor()` / `AddBridgeForMaui()` |
| Providers | 3 separate, all required | Composite `BridgeProvider` (simple) or individual providers (granular) |
| `Framework` enum | `Framework.Maui`, `.Blazor` | `Host.Maui`, `.Blazor`, `.Wpf`, `.WinForms` |
| `IBridge.Framework` | `Framework Framework` | `Host Host` |
| `ChangeListeningMode` | `.None`, `.Global`, `.Suppressed` | `ResizeMode.None`, `.Global`, `.Once` |
| `DeviceFormFactor` record | `DeviceFormFactor` | `FormFactorInfo` |
| `BridgeFramework` component | `<BridgeFramework>` | `<BridgeHost>` |
| `BridgeFrameworkHandler` | 4 variants | `BridgeHostHandler<T>` + `BridgeHostHandler` (2 variants) |
| Exception | `MauiBlazorBridgeException` | `BridgeException` |
| `BridgeFormFactorContext` | Public API | Removed |
| `IBridgeConnectivity.IsInternetConnected` | `bool` property | `IBridgeConnectivity.IsConnected` |
| `IBridgeConnectivity.InternetConnectionChanged` | Event | `IBridgeConnectivity.ConnectionChanged` |
| `IBridgeFormFactor.CreateAsync()` | Method | `IBridgeFormFactor.CreateListenerAsync()` |
| `IBridgeFormFactor.DisposeFormFactor()` | Method | `IBridgeFormFactor.DisposeListenerAsync()` |
| Target Framework | .NET 9 | .NET 10 |
| New: Theme detection | — | `IBridgeTheme`, `<BridgeTheme>` |
| New: Safe Area | — | `IBridgeSafeArea`, `<BridgeSafeArea>` |

---

**End of Proposal v2**
