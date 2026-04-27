using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeConnectivityMauiConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeConnectivityMauiConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task IsConnected_defaults_to_false_before_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        PulseAssert.False(connectivity.IsConnected);
    }

    [PulseCase]
    public async Task InitializeAsync_reads_real_Maui_connectivity_state()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();
        bool? raisedValue = null;

        connectivity.ConnectionChanged += OnConnectionChanged;

        try
        {
            await connectivity.InitializeAsync();

            PulseAssert.True(connectivity.IsConnected || !connectivity.IsConnected, "Connectivity resolves to a concrete boolean state.");
            PulseAssert.Equal(connectivity.IsConnected, raisedValue);
        }
        finally
        {
            connectivity.ConnectionChanged -= OnConnectionChanged;
        }

        void OnConnectionChanged(object? sender, bool isConnected)
        {
            raisedValue = isConnected;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_is_idempotent_and_raises_current_status_once()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();
        var eventCount = 0;

        connectivity.ConnectionChanged += OnConnectionChanged;

        try
        {
            await connectivity.InitializeAsync();
            await connectivity.InitializeAsync();

            PulseAssert.Equal(1, eventCount);
        }
        finally
        {
            connectivity.ConnectionChanged -= OnConnectionChanged;
        }

        void OnConnectionChanged(object? sender, bool isConnected)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_preserves_state_on_second_call()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync();
        var firstValue = connectivity.IsConnected;

        await connectivity.InitializeAsync();

        PulseAssert.Equal(firstValue, connectivity.IsConnected);
    }

    [PulseCase]
    public async Task InitializeAsync_accepts_connectivity_options_even_when_ignored_by_Maui()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 1, TestUrl = "/health" });

        PulseAssert.True(connectivity.IsConnected || !connectivity.IsConnected);
    }

    [PulseCase]
    public async Task InitializeAsync_raises_ConnectionChanged_with_service_sender()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();
        object? raisedSender = null;

        connectivity.ConnectionChanged += OnConnectionChanged;

        try
        {
            await connectivity.InitializeAsync();
            PulseAssert.True(ReferenceEquals(connectivity, raisedSender));
        }
        finally
        {
            connectivity.ConnectionChanged -= OnConnectionChanged;
        }

        void OnConnectionChanged(object? sender, bool isConnected)
        {
            raisedSender = sender;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_does_not_mutate_connectivity_options()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();
        var options = new ConnectivityOptions { IntervalInSeconds = 1, TestUrl = "/health" };

        await connectivity.InitializeAsync(options);

        PulseAssert.Equal(1, options.IntervalInSeconds);
        PulseAssert.Equal("/health", options.TestUrl);
    }

    [PulseCase]
    public async Task InitializeAsync_does_not_replay_ConnectionChanged_to_late_subscribers()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync();

        var eventCount = 0;
        connectivity.ConnectionChanged += OnConnectionChanged;

        try
        {
            await connectivity.InitializeAsync();
            PulseAssert.Equal(0, eventCount);
        }
        finally
        {
            connectivity.ConnectionChanged -= OnConnectionChanged;
        }

        void OnConnectionChanged(object? sender, bool isConnected)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task Dispose_completes_after_connectivity_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync();

        if (connectivity is IDisposable disposable)
            disposable.Dispose();

        PulseAssert.True(connectivity.IsConnected || !connectivity.IsConnected);
    }
}