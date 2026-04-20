# Getting Started

Circuids Bridge detects host environments, form factors, connectivity, themes, and safe areas across Blazor and MAUI Blazor Hybrid — from a single shared codebase.

## Packages

| Package | Use Case |
|---------|----------|
| `Circuids.Bridge` | Core — interfaces, components, enums. Install in shared Razor Class Libraries. |
| `Circuids.Bridge.Blazor` | Blazor WASM and Blazor Server apps (JS interop implementations). |
| `Circuids.Bridge.Maui` | MAUI Blazor Hybrid apps (native platform implementations). |

> **Tip:** `Circuids.Bridge.Blazor` and `Circuids.Bridge.Maui` both reference `Circuids.Bridge` transitively — you don't need to install the core package separately in host projects.

---

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

That's it — all five services (`IBridge`, `IBridgeFormFactor`, `IBridgeConnectivity`, `IBridgeTheme`, `IBridgeSafeArea`) are now available via DI.

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

> **Note:** Bridge services require an interactive render mode. During static SSR pre-rendering, the provider won't initialize until the circuit connects. This is by design — components will render with default values, then update once the provider initializes.

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

## Provider Configuration

### Composite provider (default — all features enabled)

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

### Selective — individual providers

For maximum control, use individual providers instead of the composite. Place them in any long-lived layout element — no nesting required:

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

## Safe Area — HTML Requirement (Blazor Only)

For `IBridgeSafeArea` to report non-zero insets on web, your HTML must include `viewport-fit=cover`:

```html
<!-- index.html or _Host.cshtml -->
<meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
```

Without this, browsers don't expose safe area insets and all values will be `0`.

---

## Next Steps

- [Usage Guide](usage.md) — Components, DI injection, handlers, and patterns
- [API Reference](api-reference.md) — Full interface and type documentation
