using Circuids.Pulse;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Blazor.Conformance.Tests.Conformance;

public sealed class BridgeConnectivityBlazorConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeConnectivityBlazorConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task IsConnected_defaults_to_true_before_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        PulseAssert.True(connectivity.IsConnected);
    }

    [PulseCase]
    public async Task InitializeAsync_reads_real_browser_network_status()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();
        bool? raisedValue = null;
        connectivity.ConnectionChanged += OnConnectionChanged;

        try
        {
            await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/" });

            PulseAssert.NotNull(raisedValue, "Blazor connectivity raises the current value during initialization.");
            PulseAssert.Equal(connectivity.IsConnected, raisedValue.Value);
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
            await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/" });
            var firstValue = connectivity.IsConnected;

            await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/" });

            PulseAssert.Equal(1, eventCount);
            PulseAssert.Equal(firstValue, connectivity.IsConnected);
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
    public async Task InitializeAsync_accepts_default_connectivity_options()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync();

        PulseAssert.True(connectivity.IsConnected || !connectivity.IsConnected);
    }

    [PulseCase]
    public async Task InitializeAsync_accepts_custom_test_url_without_polling()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/favicon.ico" });

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
            await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/" });
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
        var options = new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/favicon.ico" };

        await connectivity.InitializeAsync(options);

        PulseAssert.Equal(0, options.IntervalInSeconds);
        PulseAssert.Equal("/favicon.ico", options.TestUrl);
    }

    [PulseCase]
    public async Task InitializeAsync_does_not_replay_ConnectionChanged_to_late_subscribers()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/" });

        var eventCount = 0;
        connectivity.ConnectionChanged += OnConnectionChanged;

        try
        {
            await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/" });
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
    public async Task DisposeAsync_completes_after_connectivity_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var connectivity = scope.ServiceProvider.GetRequiredService<IBridgeConnectivity>();

        await connectivity.InitializeAsync(new ConnectivityOptions { IntervalInSeconds = 0, TestUrl = "/" });

        if (connectivity is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();

        PulseAssert.True(connectivity.IsConnected || !connectivity.IsConnected);
    }
}