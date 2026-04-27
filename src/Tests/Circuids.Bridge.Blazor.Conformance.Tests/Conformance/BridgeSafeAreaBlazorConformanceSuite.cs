using Circuids.Pulse;
using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Blazor.Conformance.Tests.Conformance;

public sealed class BridgeSafeAreaBlazorConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeSafeAreaBlazorConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task SafeArea_defaults_to_zero_before_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();

        PulseAssert.Equal(SafeAreaInsets.Zero, safeArea.SafeArea);
    }

    [PulseCase]
    public async Task InitializeAsync_reads_non_negative_safe_area_insets()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();
        SafeAreaInsets? raisedValue = null;
        safeArea.SafeAreaChanged += OnSafeAreaChanged;

        try
        {
            await safeArea.InitializeAsync();

            AssertNonNegative(safeArea.SafeArea);
            PulseAssert.Equal(safeArea.SafeArea, raisedValue);
        }
        finally
        {
            safeArea.SafeAreaChanged -= OnSafeAreaChanged;
        }

        void OnSafeAreaChanged(object? sender, SafeAreaInsets insets)
        {
            raisedValue = insets;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_is_idempotent_and_raises_SafeAreaChanged_once()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();
        var eventCount = 0;

        safeArea.SafeAreaChanged += OnSafeAreaChanged;

        try
        {
            await safeArea.InitializeAsync();
            var firstValue = safeArea.SafeArea;

            await safeArea.InitializeAsync();

            PulseAssert.Equal(1, eventCount);
            PulseAssert.Equal(firstValue, safeArea.SafeArea);
        }
        finally
        {
            safeArea.SafeAreaChanged -= OnSafeAreaChanged;
        }

        void OnSafeAreaChanged(object? sender, SafeAreaInsets insets)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_sets_HasInsets_consistently()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();

        await safeArea.InitializeAsync();

        var expected = safeArea.SafeArea.Top > 0
            || safeArea.SafeArea.Right > 0
            || safeArea.SafeArea.Bottom > 0
            || safeArea.SafeArea.Left > 0;
        PulseAssert.Equal(expected, safeArea.SafeArea.HasInsets);
    }

    [PulseCase]
    public async Task InitializeAsync_raises_SafeAreaChanged_with_service_sender()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();
        object? raisedSender = null;

        safeArea.SafeAreaChanged += OnSafeAreaChanged;

        try
        {
            await safeArea.InitializeAsync();
            PulseAssert.True(ReferenceEquals(safeArea, raisedSender));
        }
        finally
        {
            safeArea.SafeAreaChanged -= OnSafeAreaChanged;
        }

        void OnSafeAreaChanged(object? sender, SafeAreaInsets insets)
        {
            raisedSender = sender;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_does_not_replay_SafeAreaChanged_to_late_subscribers()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();

        await safeArea.InitializeAsync();

        var eventCount = 0;
        safeArea.SafeAreaChanged += OnSafeAreaChanged;

        try
        {
            await safeArea.InitializeAsync();
            PulseAssert.Equal(0, eventCount);
        }
        finally
        {
            safeArea.SafeAreaChanged -= OnSafeAreaChanged;
        }

        void OnSafeAreaChanged(object? sender, SafeAreaInsets insets)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task DisposeAsync_completes_after_safe_area_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var safeArea = scope.ServiceProvider.GetRequiredService<IBridgeSafeArea>();

        await safeArea.InitializeAsync();
        if (safeArea is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();

        AssertNonNegative(safeArea.SafeArea);
    }

    private static void AssertNonNegative(SafeAreaInsets insets)
    {
        PulseAssert.True(insets.Top >= 0);
        PulseAssert.True(insets.Right >= 0);
        PulseAssert.True(insets.Bottom >= 0);
        PulseAssert.True(insets.Left >= 0);
    }
}