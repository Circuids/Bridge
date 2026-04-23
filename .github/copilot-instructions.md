# GitHub Copilot Instructions — Circuids.Bridge

## Project Overview

Circuids.Bridge is a cross-platform Blazor library that abstracts host environment, platform, form factor, connectivity, theme, and safe area detection into a unified service layer. The same Razor components and services work identically across Blazor Server, Blazor WebAssembly, and MAUI Blazor Hybrid.

---

## Repository Structure

```
src/
  Circuids.Bridge/            # Core library: interfaces, enums, records, components, handlers
  Circuids.Bridge.Blazor/     # Blazor implementation: JS interop adapters
  Circuids.Bridge.Maui/       # MAUI implementation: native platform adapters
sample/
  Circuids.Bridge.Shared.Sample/          # Shared RCL with all sample pages
  Circuids.Bridge.Blazor.Server.Sample/   # Thin Blazor Server shell
  Circuids.Bridge.Blazor.WebAssembly.Sample/ # Thin WASM shell
  Circuids.Bridge.Maui.Sample/            # Thin MAUI Blazor Hybrid shell
docs/                         # Architecture docs, testing proposal, API reference
```

---

## Critical Coding Rules

### No C# Primary Constructors
**Never use C# primary constructors.** Always declare explicit traditional constructors with a body.

```csharp
// CORRECT
public sealed class MyService : IMyService
{
    private readonly IJSRuntime _jsRuntime;

    public MyService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
}

// WRONG — do not use primary constructors
public sealed class MyService(IJSRuntime jsRuntime) : IMyService
{
}
```

### No C# Records for Mutable State
Use `sealed record` only for immutable value objects (e.g., `FormFactorInfo`, `SafeAreaInsets`). Use `sealed class` for services and configuration objects.

---

## Namespace Conventions

| Project | Namespace |
|---|---|
| `Circuids.Bridge` | `Circuids.Bridge` |
| `Circuids.Bridge.Blazor` — public | `Circuids.Bridge.Blazor` |
| `Circuids.Bridge.Blazor` — internal | `Circuids.Bridge.Blazor.Internal` |
| `Circuids.Bridge.Maui` — public | `Circuids.Bridge.Maui` |
| `Circuids.Bridge.Maui` — internal | `Circuids.Bridge.Maui.Internal` |

Always put `@namespace Circuids.Bridge` at the top of `.razor` files in the core library.

---

## Core Interfaces and Their Contracts

All five services live in the `Circuids.Bridge` namespace:

- **`IBridge`** — host + platform detection. Properties: `Host Host`, `PlatformIdentity Platform`, `string PlatformVersion`, `bool IsInitialized`. Event: `PlatformChanged`. Method: `InitializeAsync()`.
- **`IBridgeFormFactor`** — form factor + viewport. Property: `FormFactorInfo FormFactor`. Event: `FormFactorChanged`. Methods: `InitializeAsync(ResizeMode)`, `CreateListenerAsync()`, `DisposeListenerAsync()`.
- **`IBridgeConnectivity`** — online status. Property: `bool IsConnected`. Event: `ConnectionChanged`. Method: `InitializeAsync(ConnectivityOptions?)`.
- **`IBridgeTheme`** — light/dark mode. Property: `ThemeMode Theme`. Event: `ThemeChanged`. Method: `InitializeAsync()`.
- **`IBridgeSafeArea`** — notch/cutout insets. Property: `SafeAreaInsets SafeArea`. Event: `SafeAreaChanged`. Method: `InitializeAsync()`.

---

## Common Types

### Enums
- `Host`: `Unknown | Maui | Blazor | Wpf | WinForms`
- `PlatformIdentity`: `Unknown | Android | IOS | Windows | Mac | Linux`
- `FormFactor`: `Unknown | Phone | Tablet | Desktop`
- `ThemeMode`: `Unknown | Light | Dark`
- `ResizeMode`: `None | Global | Once`

### Records (immutable value objects)
- `FormFactorInfo(FormFactor, double Width, double Height)` — factory: `FormFactorInfo.Unknown()`
- `SafeAreaInsets(double Top, double Right, double Bottom, double Left)` — static: `SafeAreaInsets.Zero`

### Classes
- `ConnectivityOptions` — `int IntervalInSeconds = 10`, `string TestUrl = "/favicon.ico"`
- `BridgeException` — the only exception type thrown by Bridge. Extends `Exception`.

---

## DI Registration Pattern

All five services are always registered as **Scoped**.

```csharp
// Blazor
services.AddScoped<IBridge, BridgeBlazor>();
services.AddScoped<IBridgeFormFactor, BridgeFormFactorBlazor>();
services.AddScoped<IBridgeConnectivity, BridgeConnectivityBlazor>();
services.AddScoped<IBridgeTheme, BridgeThemeBlazor>();
services.AddScoped<IBridgeSafeArea, BridgeSafeAreaBlazor>();

// MAUI — same pattern but Maui* implementations
```

Public extension methods live in the `Extensions/` subfolder:
- `AddBridgeForBlazor(this IServiceCollection)` in `Circuids.Bridge.Blazor`
- `AddBridgeForMaui(this IServiceCollection)` in `Circuids.Bridge.Maui`

---

## Blazor Adapter Pattern (JS Interop)

Every Blazor internal adapter follows this exact pattern:

```csharp
internal sealed class BridgeFooBlazor : IBridgeFoo, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private const string ModulePath = "./_content/Circuids.Bridge.Blazor/BridgeFoo.js";

    private bool _isInitialized;

    public BridgeFooBlazor(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;   // idempotency guard — always first

        var module = await _moduleTask.Value
            ?? throw new BridgeException("Failed to import BridgeFoo.js");

        // ... initialization logic ...

        _isInitialized = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
```

Key rules for Blazor adapters:
- Use `Lazy<Task<IJSObjectReference>>` — never store `IJSObjectReference` directly as a field.
- Always check `_isInitialized` at the top of `InitializeAsync()` before doing any work.
- Always check `_moduleTask.IsValueCreated` before awaiting in `DisposeAsync()`.
- Module paths use the `_content/{AssemblyName}/{file}.js` pattern.
- Throw `BridgeException` (not `InvalidOperationException` or `NullReferenceException`) on failure.

---

## JS→.NET Callback Pattern

When the Blazor adapter needs to receive callbacks from JavaScript:

```csharp
internal sealed class BridgeFooBlazor : IBridgeFoo, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private readonly DotNetObjectReference<BridgeFooBlazor> _dotNetRef;

    public BridgeFooBlazor(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    [JSInvokable]
    public ValueTask NotifyFooChanged(string payload)
    {
        if (!_isInitialized)
            throw new BridgeException("BridgeFoo is not initialized.");
        // ... update state and raise event ...
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
        _dotNetRef.Dispose();
    }
}
```

- `[JSInvokable]` methods must be `public`.
- Always return `ValueTask` (not `void` or `Task`) from `[JSInvokable]` methods.
- Always validate `_isInitialized` inside `[JSInvokable]` handlers.
- Always call `_dotNetRef.Dispose()` in `DisposeAsync()`.

---

## Listener Reference-Counting Pattern (FormFactor)

When an adapter manages a JS listener that may be created/destroyed multiple times:

```csharp
private int _listenerCount;
private CancellationTokenSource _cts = new();

public async Task CreateListenerAsync()
{
    CancelPendingDispose();   // abort any in-flight delayed dispose

    if (_listenerCount > 0)
    {
        _listenerCount++;
        return;
    }
    // attach JS listener ...
    _listenerCount++;
}

public async ValueTask DisposeListenerAsync()
{
    _listenerCount = Math.Max(0, _listenerCount - 1);
    if (_listenerCount > 0) return;

    // Delayed dispose: wait N seconds before actually tearing down the JS listener
    // so rapid component mount/unmount cycles don't thrash the listener.
    await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);
    // remove JS listener ...
}

private void CancelPendingDispose()
{
    _cts.Cancel();
    _cts.Dispose();
    _cts = new CancellationTokenSource();
}
```

---

## JavaScript File Pattern

All JS files in `Circuids.Bridge.Blazor/wwwroot/` use ES module syntax with named exports and no globals:

```js
// BridgeFoo.js
export function getFoo() {
    // ...
}

export function initialize(dotNetRef) {
    // register event listener, pass dotNetRef.invokeMethodAsync(...)
}

export function dispose() {
    // remove event listener
}
```

- Never use `window.*` globals or `var` declarations.
- Never use `export default`.
- All functions are named exports.
- The `dotNetRef` parameter is always the .NET object reference passed from `DotNetObjectReference.Create(this)`.

---

## MAUI Adapter Pattern

MAUI adapters wrap static MAUI APIs. They do **not** use JS interop.

```csharp
internal sealed class BridgeFooMaui : IBridgeFoo
{
    public FooValue Foo { get; private set; } = FooValue.Default;

    public event EventHandler<FooValue>? FooChanged;

    public Task InitializeAsync()
    {
        if (_isInitialized) return Task.CompletedTask;

        Foo = GetFooFromMauiApis();
        FooChanged?.Invoke(this, Foo);
        _isInitialized = true;

        return Task.CompletedTask;
    }
}
```

Key rules for MAUI adapters:
- MAUI adapters use synchronous static APIs (`DeviceInfo`, `Connectivity`, `MainThread`, `Application.Current`) — they do not inject these; they call them directly.
- Platform-conditional code uses `#if ANDROID`, `#if IOS || MACCATALYST` — only inside MAUI project files.
- MAUI adapters never implement `IAsyncDisposable` unless they own unmanaged resources.
- When dispatching to the main thread: use `MainThread.BeginInvokeOnMainThread(action)`.
- MAUI adapters are **not unit-testable** without the MAUI runtime. Accept this constraint; do not wrap static APIs for the sake of testability unless explicitly requested.

---

## Blazor Component Pattern

All Razor components in `Circuids.Bridge/Components/` follow this structure:

```razor
@namespace Circuids.Bridge
@inject ISomeService Service
@implements IDisposable   (or IAsyncDisposable for async cleanup)

@* template markup *@

@code {
    [Parameter] public RenderFragment? SomeSlot { get; set; }
    [Parameter] public RenderFragment<TModel>? ChildContent { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Service.SomeEvent += OnSomeEventChanged;
        }
    }

    private void OnSomeEventChanged(object? sender, TModel e) =>
        InvokeAsync(StateHasChanged);

    public void Dispose() => Service.SomeEvent -= OnSomeEventChanged;
}
```

Key rules:
- Subscribe to service events in `OnAfterRender(firstRender)` (not `OnInitialized`).
- For async subscriptions, use `OnAfterRenderAsync(firstRender)`.
- Always unsubscribe in `Dispose()` / `DisposeAsync()`.
- Use `InvokeAsync(StateHasChanged)` for event-driven rerenders (thread safety for Server).
- Never call `StateHasChanged()` directly from a non-render-thread callback.
- Component parameters use `RenderFragment?` (nullable) for optional slots.
- The `ChildContent` parameter can be typed: `RenderFragment<TModel>?` to pass the current model to the consumer.

---

## BridgeProvider Initialization Contract

`BridgeProvider.razor` initializes all five services **sequentially** in `OnAfterRenderAsync(firstRender)`:

```
Bridge → FormFactor → Connectivity → Theme → SafeArea
```

Child content is **not rendered** until all five services have finished initializing (`_isInitialized = true`). This is enforced by wrapping `@ChildContent` in `@if (_isInitialized)`.

Consequences:
- Never assume a Bridge service is initialized outside of a `BridgeProvider` subtree.
- Never call `InitializeAsync()` on services manually unless writing a test or custom provider.
- `BridgeProvider` parameters: `FormFactorResizeMode` (default `ResizeMode.None`), `ConnectivityOptions` (default `null`).

---

## BridgeFormFactor Component — Slot Fallback Precedence

When rendering `<BridgeFormFactor>`, the fallback precedence is:

| Active FormFactor | Slot priority (first non-null wins) |
|---|---|
| `Phone` | `Phone` → `TabletAndPhone` → `DesktopAndPhone` → `Default` |
| `Tablet` | `Tablet` → `TabletAndPhone` → `DesktopAndTablet` → `Default` |
| `Desktop` | `Desktop` → `DesktopAndTablet` → `DesktopAndPhone` → `Default` |
| `Unknown` | `Default` |

Always provide a `Default` slot as a fallback. The `ChildContent` parameter (`RenderFragment<FormFactorInfo>?`) is rendered unconditionally before the slot logic.

---

## BridgeHostHandler Pattern

To execute host-specific logic, subclass one of:

- `BridgeHostHandler` — synchronous void
- `BridgeHostHandler<T>` — synchronous with return value
- `BridgeHostHandlerAsync` — async void (`Task`)
- `BridgeHostHandlerAsync<T>` — async with return value (`Task<T>`)

```csharp
public sealed class MyHandler : BridgeHostHandler
{
    public MyHandler(IBridge bridge) : base(bridge) { }

    protected override void OnBlazor() { /* web-specific logic */ }
    protected override void OnMaui() { /* native-specific logic */ }
    // OnWpf, OnWinForms inherit OnBlazor() by default
}

// Usage
new MyHandler(bridge).Execute();
```

Rules:
- `OnBlazor()` is the **required** abstract override — it is the universal fallback.
- `OnMaui()`, `OnWpf()`, `OnWinForms()` are optional overrides; they default to calling `OnBlazor()`.
- `OnUnknown()` throws `BridgeException` by default — do not override unless there is a meaningful fallback.
- Handler classes are instantiated per-use; they are not registered in DI.

---

## Error Handling

- Throw `BridgeException` for all Bridge-layer errors (failed JS imports, uninitialized state, unknown host).
- Do not throw `InvalidOperationException`, `NullReferenceException`, or `ArgumentException` for Bridge-specific scenarios.
- Do not add `try/catch` inside service methods unless handling a specific recoverable condition.

---

## Shared Sample Architecture

The shared sample (`Circuids.Bridge.Shared.Sample`) is a Razor Class Library containing all sample pages. The three platform shells (Blazor Server, WASM, MAUI) are thin projects that call `AddBridgeSharedSample()` and add `<SharedSampleApp />` to their render tree.

When adding new sample pages:
1. Add the page to `Circuids.Bridge.Shared.Sample/Pages/`.
2. Register the route in `Circuids.Bridge.Shared.Sample/Navigation/`.
3. Do not add platform-specific code to shared sample pages.

---

## What NOT to Do

- Do not use C# primary constructors anywhere in this project.
- Do not use `#if` platform directives in `Circuids.Bridge` (core) or `Circuids.Bridge.Blazor`. Platform conditionals belong only in `Circuids.Bridge.Maui`.
- Do not register Bridge services as `Singleton` or `Transient`. Always `Scoped`.
- Do not call `StateHasChanged()` directly from a non-Blazor thread. Use `InvokeAsync(StateHasChanged)`.
- Do not place implementation classes in the root namespace of `Blazor` or `Maui` projects. Always use the `Internal/` subfolder and `internal sealed class`.
- Do not expose `IJSObjectReference` or `DotNetObjectReference` through public APIs.
- Do not add new Bridge services without adding both a Blazor and a MAUI implementation.
- Do not add global JavaScript variables or functions — all JS must use ES module named exports.
- Do not use `export default` in JavaScript files.
