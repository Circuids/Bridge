using Microsoft.Extensions.DependencyInjection;

namespace Circuids.Bridge.Maui.Conformance.Tests.Conformance;

public sealed class BridgeThemeMauiConformanceSuite
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public BridgeThemeMauiConformanceSuite(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [PulseCase]
    public async Task Theme_defaults_to_unknown_before_initialization()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var theme = scope.ServiceProvider.GetRequiredService<IBridgeTheme>();

        PulseAssert.Equal(ThemeMode.Unknown, theme.Theme);
    }

    [PulseCase]
    public async Task InitializeAsync_reads_real_Maui_theme_state()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var theme = scope.ServiceProvider.GetRequiredService<IBridgeTheme>();
        ThemeMode? raisedValue = null;

        theme.ThemeChanged += OnThemeChanged;

        try
        {
            await theme.InitializeAsync();

            PulseAssert.True(Enum.IsDefined(theme.Theme));
            PulseAssert.Equal(theme.Theme, raisedValue);
        }
        finally
        {
            theme.ThemeChanged -= OnThemeChanged;
        }

        void OnThemeChanged(object? sender, ThemeMode mode)
        {
            raisedValue = mode;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_is_idempotent_and_raises_ThemeChanged_once()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var theme = scope.ServiceProvider.GetRequiredService<IBridgeTheme>();
        var eventCount = 0;

        theme.ThemeChanged += OnThemeChanged;

        try
        {
            await theme.InitializeAsync();
            var firstTheme = theme.Theme;

            await theme.InitializeAsync();

            PulseAssert.Equal(1, eventCount);
            PulseAssert.Equal(firstTheme, theme.Theme);
        }
        finally
        {
            theme.ThemeChanged -= OnThemeChanged;
        }

        void OnThemeChanged(object? sender, ThemeMode mode)
        {
            eventCount++;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_raises_ThemeChanged_with_service_sender()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var theme = scope.ServiceProvider.GetRequiredService<IBridgeTheme>();
        object? raisedSender = null;

        theme.ThemeChanged += OnThemeChanged;

        try
        {
            await theme.InitializeAsync();
            PulseAssert.True(ReferenceEquals(theme, raisedSender));
        }
        finally
        {
            theme.ThemeChanged -= OnThemeChanged;
        }

        void OnThemeChanged(object? sender, ThemeMode mode)
        {
            raisedSender = sender;
        }
    }

    [PulseCase]
    public async Task InitializeAsync_does_not_replay_ThemeChanged_to_late_subscribers()
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var theme = scope.ServiceProvider.GetRequiredService<IBridgeTheme>();

        await theme.InitializeAsync();

        var eventCount = 0;
        theme.ThemeChanged += OnThemeChanged;

        try
        {
            await theme.InitializeAsync();
            PulseAssert.Equal(0, eventCount);
        }
        finally
        {
            theme.ThemeChanged -= OnThemeChanged;
        }

        void OnThemeChanged(object? sender, ThemeMode mode)
        {
            eventCount++;
        }
    }
}