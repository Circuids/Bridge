namespace Circuids.Bridge.TestSupport.Fakes;

public sealed class FakeBridgeTheme : IBridgeTheme
{
    public ThemeMode Theme { get; set; } = ThemeMode.Light;
    public int InitializeCallCount { get; private set; }

    public event EventHandler<ThemeMode>? ThemeChanged;

    public Task InitializeAsync()
    {
        InitializeCallCount++;
        ThemeChanged?.Invoke(this, Theme);
        return Task.CompletedTask;
    }

    public void RaiseThemeChanged(ThemeMode theme)
    {
        Theme = theme;
        ThemeChanged?.Invoke(this, theme);
    }
}
