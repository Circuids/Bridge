using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeMauiConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeMauiConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task Host_is_Maui_before_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();

        PulseAssert.Equal(Host.Maui, bridge.Host);
        PulseAssert.Equal(PlatformIdentity.Unknown, bridge.Platform);
        PulseAssert.Equal("Unknown", bridge.PlatformVersion);
        PulseAssert.False(bridge.IsInitialized);
    }

    [PulseCase]
    public async Task InitializeAsync_detects_real_Maui_platform_and_is_idempotent()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();

        await bridge.InitializeAsync();

        var firstPlatform = bridge.Platform;
        var firstVersion = bridge.PlatformVersion;

        await bridge.InitializeAsync();

        PulseAssert.True(bridge.IsInitialized);
        PulseAssert.NotEqual(PlatformIdentity.Unknown, firstPlatform, "MAUI DeviceInfo should map to a supported Bridge platform.");
        PulseAssert.Equal(firstPlatform, bridge.Platform);
        PulseAssert.Equal(firstVersion, bridge.PlatformVersion);
        PulseAssert.False(string.IsNullOrWhiteSpace(bridge.PlatformVersion));
    }

    [PulseCase]
    public async Task InitializeAsync_keeps_host_as_Maui()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();

        await bridge.InitializeAsync();

        PulseAssert.Equal(Host.Maui, bridge.Host);
        PulseAssert.True(Enum.IsDefined(bridge.Platform));
    }

    [PulseCase]
    public async Task InitializeAsync_raises_PlatformChanged()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        PlatformIdentity? raisedPlatform = null;
        bridge.PlatformChanged += OnPlatformChanged;

        try
        {
            await bridge.InitializeAsync();
            PulseAssert.Equal(bridge.Platform, raisedPlatform);
        }
        finally
        {
            bridge.PlatformChanged -= OnPlatformChanged;
        }

        void OnPlatformChanged(object? sender, PlatformIdentity platform)
        {
            raisedPlatform = platform;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_raises_PlatformChanged_with_bridge_sender()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        object? raisedSender = null;

        bridge.PlatformChanged += OnPlatformChanged;

        try
        {
            await bridge.InitializeAsync();
            PulseAssert.True(ReferenceEquals(bridge, raisedSender));
        }
        finally
        {
            bridge.PlatformChanged -= OnPlatformChanged;
        }

        void OnPlatformChanged(object? sender, PlatformIdentity platform)
        {
            raisedSender = sender;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_raises_PlatformChanged_only_once_when_called_twice()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();
        var eventCount = 0;

        bridge.PlatformChanged += OnPlatformChanged;

        try
        {
            await bridge.InitializeAsync();
            await bridge.InitializeAsync();

            PulseAssert.Equal(1, eventCount);
        }
        finally
        {
            bridge.PlatformChanged -= OnPlatformChanged;
        }

        void OnPlatformChanged(object? sender, PlatformIdentity platform)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_does_not_replay_PlatformChanged_to_late_subscribers()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var bridge = scope.ServiceProvider.GetRequiredService<IBridge>();

        await bridge.InitializeAsync();

        var eventCount = 0;
        bridge.PlatformChanged += OnPlatformChanged;

        try
        {
            await bridge.InitializeAsync();
            PulseAssert.Equal(0, eventCount);
        }
        finally
        {
            bridge.PlatformChanged -= OnPlatformChanged;
        }

        void OnPlatformChanged(object? sender, PlatformIdentity platform)
        {
            eventCount++;
        }
    }
}