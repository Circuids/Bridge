namespace Circuids.Bridge.Maui.Internal;

internal sealed class BridgeThemeMaui : IBridgeTheme, IDisposable
{
    private bool _isInitialized;

    public ThemeMode Theme { get; private set; } = ThemeMode.Unknown;

    public event EventHandler<ThemeMode>? ThemeChanged;

    public Task InitializeAsync()
    {
        if (_isInitialized) return Task.CompletedTask;

        Theme = GetTheme();

        if (Application.Current is not null)
            Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;

        _isInitialized = true;
        ThemeChanged?.Invoke(this, Theme);
        return Task.CompletedTask;
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        var mode = MapTheme(e.RequestedTheme);
        if (Theme != mode)
        {
            Theme = mode;
            ThemeChanged?.Invoke(this, mode);
        }
    }

    private static ThemeMode GetTheme()
    {
        if (Application.Current is null)
            return ThemeMode.Unknown;

        return MapTheme(Application.Current.RequestedTheme);
    }

    private static ThemeMode MapTheme(AppTheme appTheme) => appTheme switch
    {
        AppTheme.Light => ThemeMode.Light,
        AppTheme.Dark => ThemeMode.Dark,
        _ => ThemeMode.Unknown,
    };

    public void Dispose()
    {
        if (_isInitialized && Application.Current is not null)
        {
            Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
            _isInitialized = false;
        }
    }
}
