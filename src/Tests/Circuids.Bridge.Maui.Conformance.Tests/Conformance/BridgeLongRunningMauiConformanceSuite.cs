using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeLongRunningMauiConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConformanceLongRunningState _state;
    private readonly ConformanceObservationStore _observations;

    public BridgeLongRunningMauiConformanceSuite(
        IServiceScopeFactory serviceScopeFactory,
        ConformanceLongRunningState state,
        ConformanceObservationStore observations)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _state = state;
        _observations = observations;
    }

    [PulseCase(TimeoutMs = 45000)]
    public async Task Observe_runtime_state_changes_for_configured_window(CancellationToken cancellationToken)
    {
        if (!_state.IsEnabled)
            PulseAssert.Skip("Long-running state observation is disabled.");

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var formFactor = scope.ServiceProvider.GetRequiredService<IBridgeFormFactor>();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();
        var theme = scope.ServiceProvider.GetRequiredService<IBridgeTheme>();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();

        var violations = new List<string>();
        var eventCount = 0;

        bridge.PlatformChanged += OnPlatformChanged;
        formFactor.FormFactorChanged += OnFormFactorChanged;
        connectivity.ConnectionChanged += OnConnectionChanged;
        theme.ThemeChanged += OnThemeChanged;
        safeArea.SafeAreaChanged += OnSafeAreaChanged;

        try
        {
            await bridge.InitializeAsync();
            await formFactor.InitializeAsync(ResizeMode.Global);
            await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 1, TestUrl = "/" });
            await theme.InitializeAsync();
            await safeArea.InitializeAsync();

            AddSnapshot("initial", bridge, formFactor, connectivity, theme, safeArea);
            await Task.Delay(_state.ObservationDuration, cancellationToken);
            AddSnapshot("final", bridge, formFactor, connectivity, theme, safeArea);

            _observations.Add(
                "MAUI long-running summary",
                $"Observed {eventCount} service events over {_state.ObservationDuration.TotalSeconds:0} seconds.");
        }
        finally
        {
            bridge.PlatformChanged -= OnPlatformChanged;
            formFactor.FormFactorChanged -= OnFormFactorChanged;
            connectivity.ConnectionChanged -= OnConnectionChanged;
            theme.ThemeChanged -= OnThemeChanged;
            safeArea.SafeAreaChanged -= OnSafeAreaChanged;

            if (connectivity is IDisposable disposableConnectivity)
                disposableConnectivity.Dispose();

            await formFactor.DisposeListenerAsync();
        }

        if (violations.Count > 0)
            PulseAssert.Fail(string.Join(" | ", violations));

        PulseAssert.Equal(Host.Maui, bridge.Host);
        PulseAssert.True(bridge.IsInitialized);
        PulseAssert.NotEqual(PlatformIdentity.Unknown, bridge.Platform);
        PulseAssert.True(Enum.IsDefined(formFactor.FormFactor.FormFactor));
        PulseAssert.True(formFactor.FormFactor.Width >= 0);
        PulseAssert.True(formFactor.FormFactor.Height >= 0);
        PulseAssert.True(connectivity.IsConnected || !connectivity.IsConnected);
        PulseAssert.True(Enum.IsDefined(theme.Theme));
        PulseAssert.True(safeArea.SafeArea.Top >= 0);
        PulseAssert.True(safeArea.SafeArea.Right >= 0);
        PulseAssert.True(safeArea.SafeArea.Bottom >= 0);
        PulseAssert.True(safeArea.SafeArea.Left >= 0);
        PulseAssert.True(eventCount >= 5, "Initialization should emit current-value events before the observation window.");

        void OnPlatformChanged(object? sender, PlatformIdentity platform)
        {
            eventCount++;
            if (!ReferenceEquals(sender, bridge))
                violations.Add("PlatformChanged sender did not match IBridge.");
            if (bridge.Platform != platform)
                violations.Add($"PlatformChanged value {platform} did not match bridge.Platform {bridge.Platform}.");

            _observations.Add("PlatformChanged", platform.ToString());
        }

        void OnFormFactorChanged(object? sender, FormFactorInfo info)
        {
            eventCount++;
            if (!ReferenceEquals(sender, formFactor))
                violations.Add("FormFactorChanged sender did not match IBridgeFormFactor.");
            if (formFactor.FormFactor != info)
                violations.Add($"FormFactorChanged value {Format(info)} did not match service state {Format(formFactor.FormFactor)}.");

            _observations.Add("FormFactorChanged", Format(info));
        }

        void OnConnectionChanged(object? sender, bool isConnected)
        {
            eventCount++;
            if (!ReferenceEquals(sender, connectivity))
                violations.Add("ConnectionChanged sender did not match IBridgeConnectivity.");
            if (connectivity.IsConnected != isConnected)
                violations.Add($"ConnectionChanged value {isConnected} did not match service state {connectivity.IsConnected}.");

            _observations.Add("ConnectionChanged", isConnected.ToString());
        }

        void OnThemeChanged(object? sender, ThemeMode mode)
        {
            eventCount++;
            if (!ReferenceEquals(sender, theme))
                violations.Add("ThemeChanged sender did not match IBridgeTheme.");
            if (theme.Theme != mode)
                violations.Add($"ThemeChanged value {mode} did not match service state {theme.Theme}.");

            _observations.Add("ThemeChanged", mode.ToString());
        }

        void OnSafeAreaChanged(object? sender, SafeAreaInsets insets)
        {
            eventCount++;
            if (!ReferenceEquals(sender, safeArea))
                violations.Add("SafeAreaChanged sender did not match IBridgeSafeArea.");
            if (safeArea.SafeArea != insets)
                violations.Add($"SafeAreaChanged value {Format(insets)} did not match service state {Format(safeArea.SafeArea)}.");

            _observations.Add("SafeAreaChanged", Format(insets));
        }
    }

    private void AddSnapshot(
        string label,
        IBridge bridge,
        IBridgeFormFactor formFactor,
        IBridgeConnectivity connectivity,
        IBridgeTheme theme,
        IBridgeSafeArea safeArea)
    {
        _observations.Add(
            $"{label} snapshot",
            $"Host={bridge.Host}; Platform={bridge.Platform}; Version={bridge.PlatformVersion}; " +
            $"FormFactor={Format(formFactor.FormFactor)}; Connected={connectivity.IsConnected}; " +
            $"Theme={theme.Theme}; SafeArea={Format(safeArea.SafeArea)}");
    }

    private static string Format(FormFactorInfo info)
    {
        return $"{info.FormFactor} ({info.Width:0}x{info.Height:0})";
    }

    private static string Format(SafeAreaInsets insets)
    {
        return $"T={insets.Top:0.##},R={insets.Right:0.##},B={insets.Bottom:0.##},L={insets.Left:0.##}";
    }
}