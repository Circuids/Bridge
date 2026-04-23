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

---

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

---

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

---

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

---

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

## Records & Models

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
| `<BridgePlatform>` | `Android`, `IOS`, `Windows`, `Mac`, `Linux`, `Default` | `PlatformIdentity` |
| `<BridgeFormFactor>` | `Phone`, `Tablet`, `Desktop`, `DesktopAndTablet`, `DesktopAndPhone`, `TabletAndPhone`, `Default` | `FormFactorInfo` |
| `<BridgeConnectivity>` | `Online`, `Offline` | `bool` |
| `<BridgeTheme>` | `Light`, `Dark`, `Default` | `ThemeMode` |
| `<BridgeSafeArea>` | — | `SafeAreaInsets` |

---

## Providers

| Provider | Parameters | Description |
|----------|-----------|-------------|
| `<BridgeProvider>` | `FormFactorResizeMode`, `ConnectivityOptions` | Composite — initializes all services. |
| `<BridgeFormFactorProvider>` | `Mode` (`ResizeMode`) | Initializes `IBridgeFormFactor`. |
| `<BridgeConnectivityProvider>` | `Options` (`ConnectivityOptions?`) | Initializes `IBridgeConnectivity`. |
| `<BridgeThemeProvider>` | — | Initializes `IBridgeTheme`. |
| `<BridgeSafeAreaProvider>` | — | Initializes `IBridgeSafeArea`. |

---

## Handlers

### BridgeHostHandler\<T>

```csharp
public abstract class BridgeHostHandler<T>(IBridge bridge)
{
    protected abstract T OnBlazor();
    protected virtual T OnMaui() => OnBlazor();
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
    protected abstract void OnBlazor();
    protected virtual void OnMaui() => OnBlazor();
    protected virtual void OnWpf() => OnBlazor();
    protected virtual void OnWinForms() => OnBlazor();
    protected virtual void OnUnknown() => throw new BridgeException(...);
    public void Execute();
}
```

### BridgeHostHandlerAsync\<T>

```csharp
public abstract class BridgeHostHandlerAsync<T>(IBridge bridge)
{
    protected abstract Task<T> OnBlazor();
    protected virtual Task<T> OnMaui() => OnBlazor();
    protected virtual Task<T> OnWpf() => OnBlazor();
    protected virtual Task<T> OnWinForms() => OnBlazor();
    protected virtual Task<T> OnUnknown() => throw new BridgeException(...);
    public Task<T> ExecuteAsync();
}
```

### BridgeHostHandlerAsync

```csharp
public abstract class BridgeHostHandlerAsync(IBridge bridge)
{
    protected abstract Task OnBlazor();
    protected virtual Task OnMaui() => OnBlazor();
    protected virtual Task OnWpf() => OnBlazor();
    protected virtual Task OnWinForms() => OnBlazor();
    protected virtual Task OnUnknown() => throw new BridgeException(...);
    public Task ExecuteAsync();
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
