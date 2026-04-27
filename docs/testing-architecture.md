# Testing Architecture

This document describes how Bridge is tested, how the new Pulse conformance hosts fit into the test strategy, and how to add high-value coverage without coupling tests to sample apps.

For runtime design, see [architecture.md](architecture.md).

---

## Goals

Bridge tests should prove behavior at the layer where that behavior actually exists.

The test architecture is designed to:

- Keep fast unit and component tests available through `dotnet test`.
- Keep host adapter tests separate from consumer component tests.
- Exercise browser and MAUI host APIs through real host apps when fakes would hide important behavior.
- Avoid turning sample apps into test harnesses.
- Preserve existing adapter-level tests while confidence in real-host conformance grows.
- Make future coverage easy to extend by service and by host.

---

## Test Layers

| Layer | Project | Runner | Purpose |
|-------|---------|--------|---------|
| Core unit tests | `src/Tests/Circuids.Bridge.Tests` | xUnit via `dotnet test` | Core value objects, handlers, and host-independent behavior. |
| Shared support | `src/Tests/Circuids.Bridge.TestSupport` | Library | Fakes, registration inspectors, and runtime service-resolution probes. |
| Component tests | `src/Tests/Circuids.Bridge.Component.Tests` | bUnit/xUnit via `dotnet test` | Host-agnostic Razor components and providers. |
| Blazor conformance | `src/Tests/Circuids.Bridge.Blazor.Conformance.Tests` | Pulse inside Blazor WebAssembly | Browser-runtime contract tests against real JS/browser APIs. |
| MAUI conformance | `src/Tests/Circuids.Bridge.Maui.Conformance.Tests` | Pulse inside MAUI app | Device/emulator/runtime contract tests against MAUI APIs. |

`dotnet test` remains the main local gate. Pulse conformance apps are runnable app projects, so they are built by the solution but their in-app suites run inside the host runtime.

---

## Solution Wiring

All active test projects are included in `src/Circuids.Bridge.slnx` under the `Tests` folder.

The intentionally separate projects are:

- `Circuids.Bridge.Tests`
- `Circuids.Bridge.TestSupport`
- `Circuids.Bridge.Component.Tests`
- `Circuids.Bridge.Blazor.Conformance.Tests`
- `Circuids.Bridge.Maui.Conformance.Tests`

Do not reintroduce the old `Circuids.Bridge.ComponentTests` project name. Host-agnostic component coverage belongs in `Circuids.Bridge.Component.Tests`.

---

## Standard Verification Commands

Run the full fast gate:

```powershell
dotnet test src/Circuids.Bridge.slnx -c Release
```

Build the Blazor Pulse host directly:

```powershell
dotnet build src/Tests/Circuids.Bridge.Blazor.Conformance.Tests/Circuids.Bridge.Blazor.Conformance.Tests.csproj -c Release
```

Run the Blazor Pulse host locally:

```powershell
dotnet run --project src/Tests/Circuids.Bridge.Blazor.Conformance.Tests/Circuids.Bridge.Blazor.Conformance.Tests.csproj -c Release --urls http://127.0.0.1:5108
```

Then open:

```text
http://127.0.0.1:5108/conformance?autorun=1
```

The autorun page publishes JSON to `window.__bridgePulseReport` and `window.__bridgePulseReportJson` for browser automation.

To verify that the Blazor conformance runner reports failures correctly, run the opt-in failure sentinel:

```text
http://127.0.0.1:5108/conformance?autorun=1&failSentinel=1
```

That run is expected to fail exactly one intentional sentinel case. Do not use the failure sentinel URL for the normal green gate.

To observe runtime state changes over a bounded window, run the opt-in long-running suite:

```text
http://127.0.0.1:5108/conformance?autorun=1&longRun=1&longRunSeconds=15
```

This mode initializes all runtime services, listens for service events, captures initial/final snapshots, and displays the observation log in the runner UI. Keep this out of the fast gate unless the CI lane is intentionally exercising state changes.

Build the MAUI Pulse host:

```powershell
dotnet build src/Tests/Circuids.Bridge.Maui.Conformance.Tests/Circuids.Bridge.Maui.Conformance.Tests.csproj -c Release
```

On Windows, the MAUI conformance project builds Android, iOS simulator, Mac Catalyst, and Windows TFMs according to the project conditions. Running the MAUI Pulse suites requires launching the app on a supported runtime and pressing the conformance runner button.

---

## Shared Test Support

`Circuids.Bridge.TestSupport` centralizes reusable test infrastructure so tests do not each define their own fake services or Bridge service lists.

TestSupport is intentionally framework-neutral. It does not reference xUnit, bUnit, Pulse, or a runner-specific assertion package. That keeps it safe to reference from xUnit test projects and from runnable Pulse conformance host apps when there is a concrete shared helper worth using.

It contains:

- `Fakes`: public fake implementations of Bridge service contracts.
- `Contracts`: framework-neutral service registration inspection helpers.
- `Runtime`: framework-neutral service resolution probes for real host containers.

Pulse conformance host apps should reference TestSupport only when the helper proves a real host invariant. The current useful integration is `BridgeServiceResolutionProbe`, which verifies all five required Bridge services resolve from the actual host DI container. Do not add TestSupport references to conformance suites only to share trivial constants or reduce a few lines of code.

---

## Component Test Strategy

Component tests belong in `Circuids.Bridge.Component.Tests` when they validate host-agnostic Razor behavior.

This includes:

- Slot selection for `BridgeHost`, `BridgePlatform`, `BridgeConnectivity`, `BridgeTheme`, and `BridgeSafeArea`.
- Form factor fallback precedence.
- Event-driven rerender behavior.
- `BridgeProvider` initialization order.
- Provider parameter forwarding.
- Listener creation/disposal expectations from component behavior.

Component tests should use fakes from `Circuids.Bridge.TestSupport` unless a scenario needs a local recording fake to prove ordering or parameter flow.

Do not test browser APIs or MAUI APIs in this project. Those belong in adapter tests or conformance hosts.

---

## Pulse Conformance Strategy

Pulse conformance tests are in-app tests. They are not xUnit tests, not Microsoft Testing Platform apps, and not sample pages.

The package is:

```xml
<PackageReference Include="Circuids.Pulse" Version="0.1.1-experimental" />
```

Current Pulse host apps:

- `Circuids.Bridge.Blazor.Conformance.Tests`
- `Circuids.Bridge.Maui.Conformance.Tests`

Each host app:

- Registers the real Bridge host implementation.
- Registers Pulse with `AddPulse`.
- Adds service-specific suites with `pulse.AddSuite<TSuite>()`.
- Runs suites through `ITestExecutor.RunAsync()`.
- Displays summary, environment metadata, case results, and JSON output.

Pulse suites should validate public Bridge contracts against the real host runtime. Component rendering stays in bUnit/component tests, but non-rendering Bridge logic is fair game when proving it inside the installed host app adds confidence. This includes service lifetimes, event semantics, listener lifecycle rules, host-handler dispatch, option/value-object behavior, and adapter cleanup. Pulse should still avoid duplicating every fake-driven unit test; each case should either exercise real host APIs or lock down a public runtime invariant that could regress differently by host.

Long-running Pulse cases are opt-in. They are useful when observing state over time, such as connectivity changes, viewport/form-factor changes, theme changes, and safe-area updates. They should be bounded, cooperative with cancellation tokens, and skipped by default so normal conformance remains fast.

---

## Pulse Suite Pattern

Pulse suite instances may share DI state across cases. For tests that assert initialization, event raises, idempotency, or default pre-initialized state, resolve services inside a fresh scope per case.

Recommended pattern:

```csharp
public sealed class BridgeThemeBlazorConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeThemeBlazorConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task InitializeAsync_reads_real_browser_color_scheme()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var theme = scope.ServiceProvider.GetRequiredService<IBridgeTheme>();

        await theme.InitializeAsync();

        PulseAssert.True(theme.Theme is ThemeMode.Light or ThemeMode.Dark);
    }
}
```

Rules for Pulse suites:

- Use `[PulseCase]` for cases.
- Use `PulseAssert` for assertions.
- Resolve fresh scoped Bridge services per case when state matters.
- Keep cases deterministic where possible.
- Avoid xUnit references in Pulse host apps.
- Reference `Circuids.Bridge.TestSupport` only for framework-neutral helpers that validate real host behavior.
- Register new suite classes in the host app's `Program.cs` or `MauiProgram.cs`.

---

## Blazor Conformance Host

The Blazor conformance host is a Blazor WebAssembly app.

Important files:

| File | Responsibility |
|------|----------------|
| `Program.cs` | Registers `AddBridgeForBlazor()` and Pulse suites. |
| `Pages/Conformance.razor` | UI runner and autorun entry point. |
| `wwwroot/bridgePulseInterop.js` | Publishes report JSON to browser globals for automation. |
| `Conformance/*.cs` | Service-specific Pulse suites. |

The app supports manual runs from the button and automated browser runs with `?autorun=1`.

Current suites cover:

- Required Bridge service resolution, scoped reuse inside a scope, and scoped isolation across real Blazor host scopes.
- An opt-in failure sentinel that proves the runner can publish a failed Pulse report when `failSentinel=1` or `fail=1` is present.
- An opt-in long-running observation suite that listens for service events, captures state snapshots, and validates event/property consistency when `longRun=1` is present.
- Runtime contracts for `ConnectivityOptions`, `FormFactorInfo`, `SafeAreaInsets`, `BridgeHostHandler`, and async/generic host-handler variants.
- `IBridge` host/platform initialization, idempotency, event sender/count behavior, and late-subscriber behavior.
- Connectivity initialization against browser network state with default/custom options, option preservation, event sender/current-value behavior, late-subscriber behavior, and cleanup.
- Form factor viewport reads, width classification, listener preconditions, listener attachment, repeated listener creation, no-change event behavior, listener disposal no-ops, and `ResizeMode.Once`/`ResizeMode.Global` behavior.
- Theme initialization against browser color scheme, event sender/current-value behavior, late-subscriber behavior, and cleanup.
- Safe area initialization, non-negative insets, `HasInsets` consistency, event behavior, late-subscriber behavior, and cleanup.

The current Blazor Pulse browser report contains 61 conformance cases during the normal green gate: 60 passing cases and one skipped opt-in long-running observation case.

---

## MAUI Conformance Host

The MAUI conformance host is a real MAUI app, not a test project.

Important files:

| File | Responsibility |
|------|----------------|
| `MauiProgram.cs` | Registers `AddBridgeForMaui()` and Pulse suites. |
| `MainPage.xaml` | Runner UI. |
| `MainPage.xaml.cs` | Runs `ITestExecutor`, renders summary/results/JSON. |
| `Platforms/*` | Platform entry points for Windows, iOS, and Mac Catalyst. |
| `Conformance/*.cs` | Service-specific Pulse suites. |

The MAUI project uses the same TFM matrix style as the MAUI sample:

- Android always.
- iOS and Mac Catalyst when not on Linux.
- Windows when on Windows.
- `WindowsPackageType` is `None`.

Current suites cover:

- Required Bridge service resolution, scoped reuse inside a scope, and scoped isolation across real MAUI host scopes.
- An opt-in failure sentinel that proves the runner can render a failed Pulse report when the runner checkbox is enabled.
- An opt-in long-running observation suite that listens for service events, captures state snapshots, and validates event/property consistency when the runner checkbox is enabled.
- Runtime contracts for `ConnectivityOptions`, `FormFactorInfo`, `SafeAreaInsets`, `BridgeHostHandler`, and async/generic host-handler variants.
- `IBridge` host/platform initialization, idempotency, event sender/count behavior, and late-subscriber behavior.
- Connectivity initialization against MAUI connectivity state, option tolerance, option preservation, event sender/current-value behavior, late-subscriber behavior, and disposal.
- Form factor window reads, listener preconditions, listener attachment, repeated listener creation, no-change event behavior, listener disposal no-ops, and `ResizeMode.Once`/`ResizeMode.Global` behavior.
- Theme initialization against MAUI theme state, event sender/current-value behavior, and late-subscriber behavior.
- Safe area initialization, non-negative insets, `HasInsets` consistency, event sender/current-value behavior, and late-subscriber behavior.

The current MAUI Pulse app contains 58 conformance cases during the normal green run: 57 passing cases and one skipped opt-in long-running observation case. They build in the solution gate; executing them still requires launching the MAUI app on a supported runtime.

---

## What Belongs Where

| Scenario | Put It In |
|----------|-----------|
| Pure enum/value object/handler behavior | `Circuids.Bridge.Tests` |
| Razor slot selection or rerendering | `Circuids.Bridge.Component.Tests` |
| Browser runtime behavior | `Circuids.Bridge.Blazor.Conformance.Tests` |
| Device/emulator MAUI behavior | `Circuids.Bridge.Maui.Conformance.Tests` |
| Shared fake, registration inspector, or runtime probe | `Circuids.Bridge.TestSupport` |
| Usage example | `sample/Circuids.Bridge.Shared.Sample` |

Samples are not test infrastructure. Do not add Pulse, xUnit, or conformance-only UI to sample apps.

---

## Adding Coverage

When adding a new test, choose the lowest layer that can prove the behavior.

1. If the behavior is pure .NET and host-independent, add or extend unit tests first.
2. If the behavior is Razor rendering, add component tests.
3. If the behavior depends on browser/device runtime APIs, add Pulse conformance.
4. If the behavior is a public non-rendering runtime invariant that should hold inside each real host app, add matching Pulse conformance cases for Blazor and MAUI.

For a new Pulse conformance case:

1. Add the case to the service-specific suite under `Conformance/`, or create a new suite when the service area is new.
2. Inject `IServiceScopeFactory` and resolve the tested service inside the case.
3. Use `PulseAssert` and avoid xUnit assertions.
4. Register the suite in `Program.cs` or `MauiProgram.cs`.
5. Build the host app and run the relevant Pulse UI.
6. Keep the case focused on a public contract or runtime invariant.

---

## Coverage Expansion Roadmap

Future conformance work should focus on runtime behavior that fake-based tests cannot prove.

High-value candidates:

- Blazor form factor changes after viewport resize.
- Blazor connectivity transitions from online to offline and back.
- Blazor theme changes through `matchMedia` event simulation or browser automation.
- Blazor safe area behavior when `viewport-fit=cover` is present.
- MAUI window resize form factor transitions.
- MAUI connectivity change events on device/emulator.
- MAUI theme change events.
- Android and iOS safe area insets on devices with notches/cutouts.
- Provider-level conformance that verifies the full initialization sequence inside real hosts.

Avoid adding conformance cases that only restate a simple fake-driven unit test. Conformance should earn its cost by exercising the real host.

---

## CI Guidance

Minimum CI gate:

```powershell
dotnet test src/Circuids.Bridge.slnx -c Release
```

Recommended additional build gate:

```powershell
dotnet build src/Tests/Circuids.Bridge.Blazor.Conformance.Tests/Circuids.Bridge.Blazor.Conformance.Tests.csproj -c Release
dotnet build src/Tests/Circuids.Bridge.Maui.Conformance.Tests/Circuids.Bridge.Maui.Conformance.Tests.csproj -c Release
```

Recommended browser automation gate:

1. Start the Blazor conformance host.
2. Navigate to `/conformance?autorun=1`.
3. Wait for `window.__bridgePulseReport`.
4. Fail the job when `window.__bridgePulseReport.success` is not `true`.

Recommended failure-path smoke check:

1. Start the Blazor conformance host.
2. Navigate to `/conformance?autorun=1&failSentinel=1`.
3. Wait for `window.__bridgePulseReport`.
4. Pass the smoke check only when `success` is `false`, `failed` is `1`, and the failed case is `Intentional_failure_sentinel_reports_failure_when_enabled`.

Recommended long-running observation lane:

1. Start the Blazor conformance host.
2. Navigate to `/conformance?autorun=1&longRun=1&longRunSeconds=15`.
3. Wait for `window.__bridgePulseReport`.
4. Fail the job when `success` is not `true`.
5. Archive the runner page or observation output when diagnosing state-change regressions.

MAUI Pulse execution should run in a device/emulator lane when infrastructure is available. Until then, keep the app building on the solution gate and run device conformance manually for release validation.

---

## Maintenance Rules

- Keep old adapter tests until real-host conformance has been green over enough release cycles to justify removal.
- Keep Pulse apps under `src/Tests`, not under `sample`.
- Keep host-specific conformance separate by host.
- Keep TestSupport free of Pulse-specific concerns.
- Keep TestSupport free of xUnit and other runner-specific dependencies.
- Keep conformance cases resilient to suite ordering and shared scoped state.
- Update this document when adding a new test project, runner, or required verification command.
